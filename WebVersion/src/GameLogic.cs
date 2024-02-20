using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

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
            finiteStateAI.AddState(States.US_Marines, null);
            finiteStateAI.AddState(States.Shop, Shop);
            finiteStateAI.AddState(States.custom, CustomState);
            finiteStateAI.AddState(States.bombardment_meuse_argonne, MABomb);
        }

        private static StateData ChooseCharacter(StateData prev)
        {
            CharacterInformation.Reset();
            return new StateData(
                new CString[] {
                    "Choose your starting country:"
                },
                new Operation[] {
                    new Operation("[1] United States", States.US),
                    new Operation("[2] France", States.none),
                    new Operation("[3] Germany", States.none),
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
                Info.DeathCause = "an explosion during the Meuse-Argonne offensive";
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
                            //Info.CustomState = new StateData("");
                        }
                    }),
                    new Operation("[2] Pick up a shotgun", States.custom, () =>
                    {
                        Info.KillCount += 6;
                    }),
                    new Operation("[3] Pick up a grenade", States.custom, () =>
                    {
                        Info.KillCount += 10;
                    }),
                    new Operation("[4] Run away from the front line", States.custom, () =>
                    {
                        //execute
                    }),
                });

        }
        private static StateData func(StateData prev)
        {
            return new StateData(
                new CString[] {
                    ""
                },
                new Operation[] {
                    new Operation("[1] ", States.none),
                    new Operation("[2] ", States.none),
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

        ChooseCharacter,
        Shop,
        death,
        none,
        custom,
    }
}
