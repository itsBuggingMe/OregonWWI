using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace OregonWWI.Neoregon.Logic
{
    internal static class Info
    {
        public static Prob SurvivalOnAttack;
        public static Prob SurvivalOnDefence;
        public static int EquipLevel;
        public static string DeathCause;
        public static int TurnsSurvived;
        public static int KillCount;
        public static string Title;

        public static void Reset()
        {
            KillCount = 0;
            EquipLevel = 0;
            TurnsSurvived = 0;
            SurvivalOnAttack = new Prob(0.5f);
            SurvivalOnDefence = new Prob(0.5f);
        }
    }

    internal class Misc
    {
        public static GenericTextTurn Start => new GenericTextTurn(
            new CString[] {
                new CString("WWI caused over 20 million deaths. Will you be one of them?", Color.White),
                "Choose your starting country:"
            },
            new Option[] {
                new(() => ChooseUS, (Checker)'1', "[1] United States", Info.Reset),
                new(() => new ShopStage("francs(s)", 1, 10, ChooseFrench).Get(), (Checker)'2', "[2] France", () => {Info.Title = "French Conscript"; Info.Reset(); }),
                new(() => new ShopStage("papiermark(s)", 1, 10, GetCampaignGerman("Battle of Passchendaele", "Allied")   ).Get(), (Checker)'3', "[3] Germany", () => { Info.Title = "German Soldier"; Info.Reset(); }),
            });

        public static GenericTextTurn ChooseFrench => new GenericTextTurn(
            "You are sent to fight in the Battle of Verdun, the longest battle of WWI:",
            new Option[] {
                new Option(GetF1, (Checker)'1', "[1] Defend Fort Vaux"),
                new Option(GetF2, (Checker)'2', "[2] Defend Fort Douaumont"),
                new Option(() => new Desertion("You decide to run off back home!").Get(), (Checker)'3', "[3] Run Away"),
            });

        static GenericTextTurn GetF1()
        {
            Campaign c = new Campaign("Battle of Verdun", "German", End);
            c.SetEvents(c.GetEnemyAttack, c.GetGasAttack, c.FrenchEndVaux);
            return c.CampaignGetNext();
        }
        static GenericTextTurn GetF2()
        {
            Campaign c = new Campaign("Battle of Verdun", "German", End);
            if(new Prob(0.5f))
                c.SetEvents(c.GetGasAttack, c.FrenchEndDouaumont);
            else
                c.SetEvents(c.GetCharge, c.GetBombard, c.GetEnemyAttack, c.GetGasAttack, c.GetCharge, c.GetBombard, c.GetEnemyAttack, c.GetGasAttack, c.GetCharge, c.GetBombard, c.GetEnemyAttack, c.GetGasAttack);

            return c.CampaignGetNext();
        }

        public static GenericTextTurn ChooseUS => new(
            new CString[] {
                "What branch of the US military?",
            },
            new Option[] {
                new(() => new ShopStage("dollar(s)", 1, 10,  GetCampaignUS("Muse-Argonne Offensive", "German")   ).Get(), (Checker)'1', "[1] United States Army", () => Info.Title = "American infantryman"),
                new(() => new ShopStage("dollar(s)", 1, 10,  GetCampaignUS("Battle of Belleau Wood", "German")   ).Get(), (Checker)'2', "[2] United States Marines", () => { Info.SurvivalOnDefence -= 0.1f; Info.SurvivalOnDefence -= 0.05f; Info.Title = "Marine"; }),
            });

        static GenericTextTurn GetCampaignUS(string cname, string oppname)
        {
            Campaign c = new Campaign(cname, oppname, End);
            c.SetEvents(c.GetCharge, c.GetBombard, c.GetEnemyAttack, c.GetGasAttack);
            return c.CampaignGetNext();
        }

        static GenericTextTurn GetCampaignGerman(string cname, string oppname)
        {//TODO: add mines
            Campaign c = new Campaign(cname, oppname, End);
            if(new Prob(0.8f))
                c.SetEvents(c.GetEnemyAttack, c.GetBombard, c.GetEnemyAttack, c.GetBombard);
            else
                c.SetEvents(c.GetBombard, c.GetEnemyAttack, c.GetMines);

            return c.CampaignGetNext();
        }

        public static GenericTextTurn Error => new(
            new CString[] {
                "Something wrong happened",
            },
            new Option[] {
                new(() => Start, (Checker)'1', "[1] Back to start"),
            });

        public static GenericTextTurn Death => new(
            new CString[] {
                new($"You Died from {Info.DeathCause}:", Color.Red),
                "",
                $"    You Lasted {Info.TurnsSurvived} turns.",
                $"    Killed {Info.KillCount} enemies.",
                $"    Remembered as a {Info.Title}.",
                $"                                          ",
            },
            new Option[] {
                new(() => Start, (Checker)'1', "[1] Restart"),
            });

        public static GenericTextTurn End;
    }

    internal class ShopStage
    {
        int money;
        string symbol;
        int shopCnt;

        GenericTextTurn after;

        public ShopStage(string MSymbol, int min, int max, GenericTextTurn after)
        {
            this.after = after;
            symbol = MSymbol;
            money = Random.Shared.Next(min, max);

            BootOptions = new Option[]
            {
                new(Get, (Checker)'1', $"[1] Cheap Boots (1 {MSymbol})", () => money -= 1),
                new(Get, (Checker)'2', $"[2] Tough Boots (2 {MSymbol})", () =>
                {
                    money -= 2;
                    Info.SurvivalOnAttack += 0.05f;
                }),
                new(Get, (Checker)'3', $"[3] Warm Waterproof Boots (3 {MSymbol})", () =>
                {
                    money -= 3;
                    Info.SurvivalOnAttack += 0.1f;
                }),
            };

            FoodOptions = new Option[]
            {
                new(Get, (Checker)'1', $"[1] Hard Tack (1 {MSymbol})", () => money -= 1),
                new(Get, (Checker)'2', $"[2] Canned Corned Beef (2 {MSymbol})", () => {
                    money -= 2;
                    Info.SurvivalOnDefence += 0.05f;
                }),
                new(Get, (Checker)'3', $"[3] Iron Ration (3 {MSymbol})", () => {
                    money -= 3;
                    Info.SurvivalOnDefence += 0.1f;
                }),
            };

            EquipOptions = new Option[]
            {
                new(Get, (Checker)'1', $"[1] Basic Kit (1 {MSymbol})", () =>
                {
                    money -= 1;
                    Info.EquipLevel = 1;
                }),
                new(Get, (Checker)'2', $"[2] Assault Pack (2 {MSymbol})", () =>
                {
                    money -= 2;
                    Info.SurvivalOnDefence += 0.03f;
                    Info.SurvivalOnAttack += 0.03f;
                    Info.EquipLevel = 2;
                }),
                new(Get, (Checker)'3', $"[3] Assault Pack with Helmet (3 {MSymbol})", () => {
                    money -= 3;
                    Info.SurvivalOnDefence += 0.05f;
                    Info.SurvivalOnAttack += 0.05f;
                    Info.EquipLevel = 3;
                }),
            };

            ShopPurchases = new Option[][] { BootOptions, FoodOptions, EquipOptions };
        }

        Option[] BootOptions;

        Option[] FoodOptions;

        Option[] EquipOptions;

        Option[][] ShopPurchases;

        public GenericTextTurn Get()
        {
            if (money == 0 || shopCnt == 3)
            {
                return after;
            }

            return new GenericTextTurn(
                new CString[] {
                    $"You have {money} {symbol}. You can buy:"
                },
                    ShopPurchases[shopCnt++][..Math.Min(money, 3)]
                );
        }
    }

    internal class Campaign
    {
        public static Campaign ActiveCampaign { get; private set;}

        private string offName;
        private string enname;

        private Func<GenericTextTurn>[] Events;
        private int index;
        GenericTextTurn end;
        public Campaign(string offsensiveName, string enemyName, GenericTextTurn endState, params Func<GenericTextTurn>[] Events)
        {
            end = endState;
            this.Events = Events;
            offName = offsensiveName;
            enname = enemyName;
            ActiveCampaign = this;
        }

        public void SetEvents(params Func<GenericTextTurn>[] Events)
        {
            this.Events = Events;
        }

        public GenericTextTurn GetCharge()
        {
            GenericTextTurn Survive()
            {
                return new(
                    new CString[] {
                        "You Survived the charge, but barely",
                    },
                    new Option[] {
                        new(CampaignGetNext, (Checker)'1', "[1] Move onto next week"),
                    });
            }

            return new(
                new CString[] {
                    $"You are ordered charge the {enname} lines at {offName}.",
                },
                new Option[] {
                    new(() => ProbDeath($"Charging in {offName}", Info.SurvivalOnAttack, Survive()), (Checker)'1', "[1] Charge forwards with the squad"),
                    new(Survive, (Checker)'2', "[2] Man the defenses"),
                });
        }

        public GenericTextTurn FrenchEndVaux()
        {
            var a = new GenericTextTurn(
                    new CString[] {
                        "It was all quiet until                                                        ",
                        new CString("BOOOOM", Color.Yellow, offsets: new Vector2["BOOOOM".Length]),
                        "An artillery shell exploded near you"
                    },
                    new Option[] {
                        new(() => CampaignGetDeath("being blown up by Artillery"), (Checker)'1', "[1] Continue"),
                    }).AddBoom(1);



            return a;
        }

        public GenericTextTurn FrenchEndDouaumont()
        {
            return new(
                    new CString[] {
                        "There is a German assault, and you are killed                                                        ",
                        "Unlucky."
                    },
                    new Option[] {
                        new(() => CampaignGetDeath("killed by a German Assault in the Battle of Verdun"), (Checker)'1', "[1] Continue"),
                    });
        }

        public GenericTextTurn GetMines()
        {
            return new GenericTextTurn(
                new CString[] {
                    "You hear rumors of tunneling under the trenches                                        ",
                    "Funny, what a silly thou-",
                    new CString("BOOOOM", Color.Yellow, offsets: new Vector2["BOOOOM".Length])
                },
                new Option[] {
                    new(() => CampaignGetDeath("being blown up by British mines"), (Checker)'1', "[1] Continue"),
                }).AddBoom(2);
        }

        public GenericTextTurn GetBombard()
        {
            GenericTextTurn Survive()
            {
                return new(
                new CString[] {
                    "Bombs whistle all around you                                               ",
                    new CString(". . .                                                                      ", Color.Red),
                    "You hear screams, but after a few hours, it is over and you survive.",                },
                    new Option[] {
                        new(CampaignGetNext, (Checker)'1', "[1] Move onto next week"),
                    });
            }

            return new(
                new CString[] {
                    $"There is a sudden {enname} bombardment.",
                },
                new Option[] {
                    new(() => ProbDeath($"Blown up in the {offName}", Info.SurvivalOnDefence, Survive()), (Checker)'1', "[1] Man the Defenses"),
                    new(Survive, (Checker)'2', "[2] Enter Bunker"),
                });
        }

        public GenericTextTurn GetEnemyAttack()
        {
            GenericTextTurn Survive(string numKilled)
            {
                return new(
                    new CString[] {
                        $"You survived, and managed to kill {numKilled} people",
                        "You no longer think the war is so glorious", 
                    },
                    new Option[] {
                        new(CampaignGetNext, (Checker)'1', "[1] Move onto next week"),
                    });
            }

            return new(
                new CString[] {
                    $"There is a sudden {enname} attack. You have little time to react.",
                },
                new Option[] {
                    new(() => ProbDeath($"Defending in the {offName}", new Prob(0.6f), Survive("two")), (Checker)'1', "[1] Pick up a rifle with bayonet", () => Info.KillCount += 2),
                    new(() => ProbDeath($"Defending in the {offName}", new Prob(0.4f), Survive("six")), (Checker)'2', "[2] Pick up a shotgun", () => Info.KillCount += 6),
                    new(() => ProbDeath($"Defending in the {offName}", new Prob(0.3f), Survive("ten")), (Checker)'3', "[3] Pick up a grenade", () => Info.KillCount += 10),
                    new(() => new Desertion($"You decide to run away from the {enname}s").Get(), (Checker)'4', "[4] Run away from the front line"),
                });
        }

        public GenericTextTurn GetGasAttack()
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

            string message = GetStringFromEquip(out bool survives);

            return new(
                new CString[] {
                    "It was a calm day when you heard shouts of-",
                    new("GAS! GAS!", Color.YellowGreen),
                    message
                },
                new Option[] {
                    (survives ? new Option(CampaignGetNext, (Checker)'1', "[1] Continue") : new Option(() => CampaignGetDeath("being unprepared in a gas attack"), (Checker)'1', "Continue"))
                });

        }
        public GenericTextTurn CampaignGetNext()
        {
            if (Events.Length == index)
            {
                return end;
            }
            return Events[index++]();
        }

        public GenericTextTurn ProbDeath(string cause, Prob prob, GenericTextTurn inCase)
        {
            if(prob)
            {
                return CampaignGetNext();
            }
            return CampaignGetDeath(cause);
        }

        public GenericTextTurn CampaignGetDeath(string cause)
        {
            Info.DeathCause = cause;
            return Misc.Death;
        }
    }

    internal static class Extension
    {
        public static GenericTextTurn AddBoom(this GenericTextTurn a, int lineNum)
        {
            string s = a.Text[lineNum].String;
            int charTotal = 30;
            for(int i = 0; i < lineNum; i++)
            {
                charTotal += a.Text[i].Length;
            }
            Vector2[] Vel = Enumerable.Range(0, s.Length).Select(
                c =>
                new Vector2(Random.Shared.NextSingle() * 2 - 1, Random.Shared.NextSingle() * 2 - 1) * 0.8f
                ).ToArray();

            a.SetAnimation(new Animation(Update, 360, animationType: AnimationType.InverseCubic));

            void Update(float f)
            {
                charTotal--;

                if(charTotal > 0)
                {
                    return;
                }
                var locref = a.Text[lineNum].offsets;
                for (int i = 0; i < Vel.Length; i++)
                {
                    Vel[i] *= 0.97f;
                    locref[i] += Vel[i];
                }
            }
            return a;
        }
    }

    internal class Desertion
    {
        private string decision;

        public Desertion(string decision)
        {
            this.decision = decision;
        }

        public GenericTextTurn Get()
        {
            return new(
            new CString[] {
                decision,
                "You are finally free from the horrors of war!",
            },
            new Option[] {
                new(() => new GenericTextTurn(
                    new CString[] {
                        "The military finds out about your actions",
                        "You are brought before the military courts",
                        "The Verdict is .     .      .                                                                                      ",
                        "You are charged with cowardice and will be executed by firing squad",
                    },
                    new Option(BlamAndDeath, (Checker)'1', "[1] Continue")
                    ), (Checker)'1', "[1] Enjoy your newfound freedom"),
            });
        }

        private GenericTextTurn BlamAndDeath()
        {
            Info.DeathCause = "being executed for desertion";
            return new GenericTextTurn(
                new CString[] {
                    ".   .   .",
                    "                                                                                                   ",
                    new("BANG", Color.Red),
                    "                                                                                                   ",
                    "         ",
                },
                new Option(() => Misc.Death, (Checker)'1', new CString("[1] Continue", Color.White)));
        }
    }
}
