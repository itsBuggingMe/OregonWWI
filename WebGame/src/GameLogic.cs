using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace OregonWWI
{
    internal static class GameLogic
    {
        private static CharacterInformation Info => CharacterInformation.Info;

        public static void Initalize(FiniteStateAI<States> finiteStateAI)
        {
            finiteStateAI.AddState(States.ChooseCharacter, ChooseCharacter);
            finiteStateAI.AddState(States.death, Death);
            finiteStateAI.AddState(States.US, US);
            finiteStateAI.AddState(States.US_Army, US_Army);
            finiteStateAI.AddState(States.US_Marines, Marine);
            finiteStateAI.AddState(States.Shop, Shop);
            finiteStateAI.AddState(States.custom, CustomState);
            finiteStateAI.AddState(States.bombardment_meuse_argonne, MABomb);
            finiteStateAI.AddState(States.enemyAttack_meuse_argonne, enemyAttckMuse);
            finiteStateAI.AddState(States.gas_attack, GasAttack);
            finiteStateAI.AddState(States.cowardice, Cowardice);
            finiteStateAI.AddState(States.survive, Survive);
            finiteStateAI.AddState(States.french, LolNoSurvive);
            finiteStateAI.AddState(States.Passchendaele, Passchendaele);
            finiteStateAI.AddState(States.EATTACK_Passchendaele, EATTACK_Passchendaele);

            Task.Run(() => str = get());
        }

        private static StateData ChooseCharacter(StateData prev)
        {
            CharacterInformation.Reset();
            return new StateData(
                new CString[] {
                    "WWI caused over 20 million deaths. Will you be one of them?",
                    "Choose your starting country:"
                },
                new Operation[] {
                    new Operation("[1] United States", States.US),
                    new Operation("[2] France", States.french),
                    new Operation("[3] Germany", States.Passchendaele),
                });
        }

        private static StateData US(StateData prev)
        {
            return new StateData(
                new CString[] {
                    "What branch of the US military?"
                },
                new Operation[] {
                    new Operation("[1] United States Army", States.US_Army),
                    new Operation("[2] United States Marines", States.US_Marines),
                });
        }

        private static StateData US_Army(StateData prev)
        {
            bool attackSurvive = Info.SurvivalOnAttack;
            Info.OffensiveName = "Meuse-Argonne offensive";

            States next = attackSurvive ? States.custom : States.death;
            if(!attackSurvive)
            {
                Info.DeathCause = "Charging in the Meuse-Argonne offensive";
            }
            else
            {
                Info.CustomState = new StateData(
                new CString[] {
                    "You Survived the charge, but barely"
                },
                new Operation[] {
                    new Operation("[1] Move onto next week", States.bombardment_meuse_argonne),
                });
            }
            Info.Title = "Infantryman in the US Army";



            CharacterInformation.Info.AfterShop = new StateData(
                new CString[] {
                    "You are deployed to the Meuse-Argonne offensive:"
                },
                new Operation[] {
                    new Operation("[1] Charge Forward with the squad", next),
                    new Operation("[2] Man the defenses", States.custom, () =>
                    {
                        Info.Cowardice++;
                        Info.CustomState = new StateData(
                            new CString[] {
                                "You Survived defending the base."
                            },
                            new Operation[] {
                                new Operation("[1] Move onto next week", States.bombardment_meuse_argonne),
                            });
                    }),
                });

            return new StateData(
                new CString[] {
                    "You enlist in the US Army"
                },
                new Operation[] {
                    new Operation("[1] Go to buy Boots, Extra Food, and Equipment", States.Shop),
                });
        }

        #region Shop
        static Operation[] BootOptions = new Operation[]
        {
            new Operation("[1] Cheap Boots ($1)", States.Shop, () => Info.Money -= 1),
            new Operation("[2] Tough Boots ($2)", States.Shop, () =>
            {
                Info.Money -= 2;
                Info.SurvivalOnAttack += 0.05f;
            }),
            new Operation("[3] Warm Waterproof Boots ($3)", States.Shop, () =>
            {
                Info.Money -= 3;
                Info.SurvivalOnAttack += 0.1f;
            }),
        };

        static Operation[] FoodOptions = new Operation[]
        {
            new Operation("[1] Hard Tack ($1)", States.Shop, () => Info.Money -= 1),
            new Operation("[2] Canned Corned Beef ($2)", States.Shop, () => { 
                Info.Money -= 2; 
                Info.SurvivalOnDefence += 0.05f;
            }),
            new Operation("[3] Iron Ration ($3)", States.Shop, () => { 
                Info.Money -= 3; 
                Info.SurvivalOnDefence += 0.1f;
            }),
        };

        static Operation[] EquipOptions = new Operation[]
        {
            new Operation("[1] Basic Kit ($1)", States.Shop, () =>
            {
                Info.Money -= 1;
                Info.EquipLevel = 1;
            }),
            new Operation("[2] Assault Pack ($2)", States.Shop, () => 
            { 
                Info.Money -= 2; 
                Info.SurvivalOnDefence += 0.03f;
                Info.SurvivalOnAttack += 0.03f;
                Info.EquipLevel = 2;
            }),
            new Operation("[3] Assault Pack with Helmet ($3)", States.Shop, () => { 
                Info.Money -= 3;
                Info.SurvivalOnDefence += 0.05f;
                Info.SurvivalOnAttack += 0.05f;
                Info.EquipLevel = 3;
            }),
        };

        static Operation[][] ShopPurchases = new Operation[][] { BootOptions, FoodOptions, EquipOptions };
        #endregion

        private static StateData Shop(StateData prev)
        {
            if(Info.Money == 0 || Info.ShopItemCount == 3)
            {
                return Info.AfterShop;
            }

            return new StateData(
                new CString[] {
                    $"You have {Info.Money}$. You can buy:"
                },
                    ShopPurchases[Info.ShopItemCount++][..Math.Min(Info.Money, 3)]
                );
        }

        private static StateData Death(StateData prev)
        {
            return new StateData(
                new CString[] {
                    new CString(Color.Red, $"You Died from {Info.DeathCause}:"),
                    "",
                    $"    You Lasted {Info.TurnsSurvived} turns.",
                    $"    Had {Info.Money} dollar(s).",
                    $"    Killed {Info.KillCount} enemies.",
                    $"    Remembered as a {Info.Title}.",
                    $"                                                    ",
                },
                new Operation[] {
                    new Operation("[1] Restart", States.ChooseCharacter),
                });
        }

        private static StateData CustomState(StateData prev)
        {
            return Info.CustomState;
        }

        private static StateData MABomb(StateData prev)
        {
            bool defSurvive = Info.SurvivalOnDefence;

            States next = defSurvive ? States.custom : States.death;
            if (!defSurvive)
            {
                Info.DeathCause = $"an explosion during the {Info.OffensiveName}";
            }
            else
            {
                Info.CustomState = new StateData(
                new CString[] {
                    "Bombs whistle all around you                                               ",
                    new CString(Color.Red, ". . .                                                                      "),
                    "You hear screams, but after a few hours, it is over and you survive.",
                },
                new Operation[] {
                    new Operation("[1] Move onto next week", States.enemyAttack_meuse_argonne),
                });
            }

            return new StateData(
                new CString[] {
                    "There is sudden bombardment of your position."
                },
                new Operation[] {
                    new Operation("[1] Man the Defenses", next),
                    new Operation("[2] Enter Bunker", States.custom, () =>
                    {
                        Info.Cowardice++;
                        Info.CustomState = new StateData(
                            new CString[] {
                                "You survived, hiding in the bunker, safe and sound.",
                                "Another soldier scolds you for hiding."
                            },
                            new Operation[] {
                                new Operation("[1] Move onto next week", States.enemyAttack_meuse_argonne),
                            });
                    }),
                });
        }

        private static StateData enemyAttckMuse(StateData prev)
        {
            return new StateData(
                new CString[] {
                    "There is a sudden german attack. You have little time to react."
                },
                new Operation[] { // determine data in info!!!
                    new Operation("[1] Pick up a rifle with bayonet", States.custom, () =>
                    {
                        Info.KillCount += 2;
                        if(new Prob(0.4f))
                        {//survives
                            Info.CustomState = new StateData(
                                new CString[] {
                                    "You survived, and managed to kill two people",
                                    "You no longer think the war is so glorious",
                                },
                                new Operation[] {
                                    new Operation("[1] Move onto next week", States.gas_attack),
                                });
                        }
                        else
                        {
                            Info.DeathCause = "defending a German Attack";
                            Info.CustomState = Death(null);
                        } 
                    }),
                    new Operation("[2] Pick up a shotgun", States.custom, () =>
                    {
                        Info.KillCount += 6;
                        if(new Prob(0.6f))
                        {//survives
                            Info.CustomState = new StateData(
                                new CString[] {
                                    "You survived, and managed to kill six people",
                                    "You no longer think the war is so glorious",
                                },
                                new Operation[] {
                                    new Operation("[1] Move onto next week", States.gas_attack),
                                });
                        }
                        else
                        {
                            Info.DeathCause = "defending a German Attack";
                            Info.CustomState = Death(null);
                        }    
                    }),
                    new Operation("[3] Pick up a grenade", States.custom, () =>
                    {
                        Info.KillCount += 10;
                        if(new Prob(0.6f))
                        {//survives
                            Info.CustomState = new StateData(
                                new CString[] {
                                    "You survived, and managed to kill six people",
                                    "You no longer think the war is so glorious",
                                },
                                new Operation[] {
                                    new Operation("[1] Move onto next week", States.gas_attack),
                                });
                        }
                        else
                        {
                            Info.DeathCause = "defending your position, with only a grenade";
                            Info.CustomState = Death(null);
                        }
                    }),
                    new Operation("[4] Run away from the front line", States.custom, () =>
                    {
                        //execute
                        if(new Prob(0.9f))
                        {//survives
                            Info.CustomState = new StateData(
                                new CString[] {
                                    "You quickly run from the front lines. You are Free!",
                                },
                                new Operation[] {
                                    new Operation("[1] Continue with your newfound freedom", States.cowardice),
                                });
                        }
                        else
                        {
                            Info.DeathCause = "shot by a stray bullet in an attempt to desert";
                            Info.CustomState = Death(null);
                        }
                    }),
                });

        }

        private static StateData GasAttack(StateData prev)
        {
            string GetStringFromEquip(out bool survives)
            {
                survives = false;
                if (Info.EquipLevel == 1)
                    return "You realize you did not buy a gas mask.";

                survives = new Prob(0.6f);
                if (Info.EquipLevel == 2)
                    return "You manage to find a wet rag to protect yourself";

                survives = true;
                return "You grab your gas mask, and survives. Others were not as fourtunate";
            }

            static bool Set(bool @in)
            {
                Info.DeathCause = "being unprepared";
                return @in;
            }

            return new StateData(
                new CString[] {
                    "It was a calm day when you heard shouts of-",
                    new(Color.YellowGreen, "GAS! GAS!"),
                    GetStringFromEquip(out bool survives),
                },
                new Operation[] {
                    new Operation("[1] Continue", Set(survives) ? States.survive : States.death),
                });
        }

        private static StateData Cowardice(StateData prev)
        {
            Info.CustomState = new StateData(
                new CString[] {
                    ".   .   .",
                    "                                                                                                   ",
                    new(Color.Red, "BANG"),
                    "                                                                                                   ",
                    "         ",
                },
                new Operation[] {
                    new Operation("[1] Continue", States.death),
                });
            Info.DeathCause = "being executed in dishonor";

            return new StateData(
                new CString[] {
                    "The military finds out about your actions",
                    "You are brought before the military courts",
                    "The Verdict is .     .      .                                                                                      ",
                    "You are charged with cowardice and will be executed by firing squad",
                },
                new Operation[] {
                    new Operation("[1] Continue", States.custom),
                });
        }

        static string[] str;

        static string[] get()
        {
            try
            {
                return JsonConvert.DeserializeObject<string[]>(c.GetAsync("https://highscores.neonrogue.net/map/wwi").Result.Content.ReadAsStringAsync().Result);
            }
            catch
            {
                return null;
            }
        }

        private static StateData Survive(StateData prev)
        {
            var arr = new CString[] {
                    "Other Survivors, just like you:"
                }.ToList();
            if(str != null)
            {
                arr.AddRange(str?.Select(c => (CString)c));
            }

            Info.CustomState = new StateData(
                arr.ToArray(),
                new Operation[] {
                    new Operation("[1] Restart", States.ChooseCharacter),
                });

            return new StateData(
                new CString[] {
                    "Congratulations! You survived WWI"
                },
                new Operation[] {
                    new Operation("[1] Type your name here, prefixed by a \"1\" e.g. 1Jason", States.custom, ()=>
                    {
                        string user = Info.UserInput[1..];
                        if(!string.IsNullOrWhiteSpace(user))
                        {
                            c.PostAsync("https://highscores.neonrogue.net/uploadmap/wwi", new StringContent($"{user} as {Info.Title} with {Info.KillCount} kills."));
                        }
                    }),
                });
        }

        static HttpClient c = new();

        private static StateData Marine(StateData prev)
        {
            Info.SurvivalOnDefence -= new Prob(0.1f);
            Info.SurvivalOnAttack -= new Prob(0.1f);

            bool attackSurvive = Info.SurvivalOnAttack;

            States next = attackSurvive ? States.custom : States.death;
            if (!attackSurvive)
            {
                Info.DeathCause = "Defending in the Battle of Belleau Wood";
            }
            else
            {
                Info.CustomState = new StateData(
                new CString[] {
                    "You survived while defending."
                },
                new Operation[] {
                    new Operation("[1] Move onto next week", States.bombardment_meuse_argonne),
                });
            }
            Info.Title = "Marine in the US Army";



            CharacterInformation.Info.AfterShop = new StateData(
                new CString[] {
                    "You are deployed to Belleau Wood:"
                },
                new Operation[] {
                    new Operation("[1] Charge Forward with the squad", next),
                    new Operation("[2] Man the defenses", States.custom, () =>
                    {
                        Info.Cowardice++;
                        Info.CustomState = new StateData(
                            new CString[] {
                                "You Survived defending the base."
                            },
                            new Operation[] {
                                new Operation("[1] Move onto next week", States.bombardment_meuse_argonne),
                            });
                    }),
                });

            return new StateData(
                new CString[] {
                    "You are part of the US Marines"
                },
                new Operation[] {
                    new Operation("[1] Go to buy Boots, Extra Food, and Equipment", States.Shop),
                });
        }

        private static StateData LolNoSurvive(StateData prev)
        {
            Info.Title = "French conscript";

            CharacterInformation.Info.AfterShop = new StateData(
                new CString[] {
                    "You are sent to fight in the Battle of Verdun, the longest battle of WWI:"
                },
                new Operation[] {
                    new Operation("[1] Defend Fort Vaux", States.custom, () =>
                    {
                        Info.DeathCause = "being blown up by Artillery";
                        Info.CustomState = new StateData(
                            new CString[] {
                                "It was all quiet until                                                        ",
                                new CString(Color.Yellow, "BOOOOM"),
                                "An artillery shell exploded near you"
                            },
                            new Operation[] {
                                new Operation("[1] Continue", States.death),
                            });
                    }),
                    new Operation("[2] Defend Fort Douaumont", States.custom, () =>
                    {
                        Info.DeathCause = "killed by a German Assault in the Battle of Verdun";
                        Info.CustomState = new StateData(
                            new CString[] {
                                "There is a German assault, and you are killed                                                        ",
                                "Unlucky."
                            },
                            new Operation[] {
                                new Operation("[1] Continue", States.death),
                            });
                    }),
                    new Operation("[3] Run away", States.custom, () =>
                    {
                        Info.CustomState = new StateData(
                            new CString[] {
                                "Miraculously, you survive.",
                                "You decide to make your way back home",
                            },
                            new Operation[] {
                                new Operation("[1] Continue", States.cowardice),
                            });
                    }),
                });

            return new StateData(
                new CString[] {
                    "You are conscripted as a French soldier in early 1916."
                },
                new Operation[] {
                    new Operation("[1] Go to buy Boots, Extra Food, and Equipment", States.Shop),
                });
        }

        private static StateData Passchendaele(StateData prev)
        {
            Info.Title = "German soldier";

            bool attackSurvive = Info.SurvivalOnAttack;
            Info.OffensiveName = "Battle of Passchendaele";

            CharacterInformation.Info.AfterShop = new StateData(
                new CString[] {
                    "You are deployed to the Battle of Passchendaele:",
                    "Within a week, there is an Allied assault"
                },
                new Operation[] {
                    new Operation("[1] Run away", States.custom, () =>
                    {
                        Info.CustomState = new StateData(
                            new CString[] {
                                "You ran into the Belgium countryside"
                            },
                            new Operation[] {
                                new Operation("[1] Travel back home", States.cowardice),
                            });
                    }),
                    new Operation("[2] Throw grenade", States.custom, () =>
                    {
                        Info.DeathCause = "Fighting in the Battle of Passchendaele";
                        Info.KillCount+=5;
                        if(new Prob(0.7f))
                        {
                            Info.CustomState = new StateData(
                                new CString[] {
                                    "You Barely survive"
                                },
                                new Operation[] {
                                    new Operation("[1] Move onto next week", new Prob(0.5f) ? States.bombardment_meuse_argonne : States.EATTACK_Passchendaele),
                                });
                        }
                        else
                        {
                            Info.CustomState = Death(null);
                        }
                    }),
                    new Operation("[3] Shoot machine gun", States.custom, () =>
                    {
                        Info.KillCount += Random.Shared.Next(10,15);
                        Info.DeathCause = "Fighting in the Battle of Passchendaele";
                        if(new Prob(0.7f))
                        {
                            Info.CustomState = new StateData(
                                new CString[] {
                                    "You brutally gun down many allied soldiers"
                                },
                                new Operation[] {
                                    new Operation("[1] Move onto next week", new Prob(0.5f) ? States.bombardment_meuse_argonne : States.EATTACK_Passchendaele),
                                });
                        }
                        else
                        {
                            Info.CustomState = Death(null);
                        }
                    }),
                });


            return new StateData(
                new CString[] {
                    "You join the Imperial German Army"
                },
                new Operation[] {
                    new Operation("[1] Go to buy Boots, Extra Food, and Equipment", States.Shop),
                });
        }

        private static StateData EATTACK_Passchendaele(StateData prev)
        {
            Info.DeathCause = "being blown up by British mines";
            return new StateData(
                new CString[] {
                    "You hear rumors of tunneling under the trenches                                        ",
                    "Funny, what a silly thou-",
                    new CString(Color.Yellow, "BOOOOM")
                },
                new Operation[] {
                    new Operation("[1] Continue", States.death),
                });
        }
    }

    internal enum States
    {
        US,
        US_Army,
        US_Marines,
        meuse_argonne,
        Belleau_wood,
        charge_meuse_argonne,
        bombardment_meuse_argonne,
        enemyAttack_meuse_argonne,
        gas_attack,

        french,

        Passchendaele,
        D1_Passchendaele,
        EATTACK_Passchendaele,
        D2_Passchendaele,

        ChooseCharacter,
        Shop,
        death,
        none,
        custom,
        cowardice,
        survive
    }
}
