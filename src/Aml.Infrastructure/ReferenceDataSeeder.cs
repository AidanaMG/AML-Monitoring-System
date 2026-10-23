using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Aml.Application.Screening;
using System.Text.Json;



namespace Aml.Infrastructure
{
    public static class ReferenceDataSeeder
    {
        private static readonly DateTime SeedDate = new(2026, 1, 1);


        public static async Task SeedAsync(AmlDbContext db, CancellationToken ct)
        {
            
            if(!await db.Countries.AnyAsync(ct))
            {
                var countries = new List<Country>{
                    // 0–3.9 → Low, 4–5.9 → Medium, 6+ → High, FATF blacklist → Critical

                    // Low Risk
                    new() { Code = "KZ", Name = "Kazakhstan",           RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },
                    new() { Code = "FR", Name = "France",               RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },
                    new() { Code = "FI", Name = "Finland",              RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },
                    new() { Code = "SE", Name = "Sweden",               RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },
                    new() { Code = "SI", Name = "Slovenia",             RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },
                    new() { Code = "CZ", Name = "Czech Republic",       RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },
                    new() { Code = "AM", Name = "Armenia",              RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },
                    new() { Code = "PT", Name = "Portugal",             RiskLevel = RiskLevel.Low,     FatfListed = false,  RiskDetails = null },


                    // Medium Risk
                    new() { Code = "GB", Name = "United Kingdom",       RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "DE", Name = "Germany",              RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "US", Name = "United States",        RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "KG", Name = "Kyrgyzstan",           RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "UZ", Name = "Uzbekistan",           RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "TR", Name = "Turkey",               RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "UA", Name = "Ukraine",              RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "GE", Name = "Georgia",              RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "AZ", Name = "Azerbaijan",           RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "PA", Name = "Panama",               RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "SC", Name = "Seychelles",           RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "TH", Name = "Thailand",             RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
                    new() { Code = "BY", Name = "Belarus",              RiskLevel = RiskLevel.Medium,  FatfListed = false, RiskDetails = null },
 
                    // High Risk
                    new() { Code = "IQ", Name = "Iraq",                 RiskLevel = RiskLevel.High,    FatfListed = true,   RiskDetails = null},
                    new() { Code = "TM", Name = "Turkmenistan",         RiskLevel = RiskLevel.High,    FatfListed = false,  RiskDetails = null},
                    new() { Code = "TJ", Name = "Tajikistan",           RiskLevel = RiskLevel.High,    FatfListed = false,  RiskDetails = null},
                    new() { Code = "AE", Name = "United Arab Emirates", RiskLevel = RiskLevel.High,    FatfListed = false,  RiskDetails = null},
                    new() { Code = "CN", Name = "China",                RiskLevel = RiskLevel.High,    FatfListed = false,  RiskDetails = null},
                    new() { Code = "RU", Name = "Russia",               RiskLevel = RiskLevel.High,    FatfListed = false,  RiskDetails = null},
                    new() { Code = "SY", Name = "Syria",                RiskLevel = RiskLevel.High,    FatfListed = true,   RiskDetails = CountryRiskTag.Drug},
                    new() { Code = "AF", Name = "Afghanistan",          RiskLevel = RiskLevel.High,    FatfListed = false,  RiskDetails = CountryRiskTag.Drug},
                    new() { Code = "VN", Name = "Vietnam",              RiskLevel = RiskLevel.High,    FatfListed = true,   RiskDetails = null},
                    new() { Code = "KE", Name = "Kenya",                RiskLevel = RiskLevel.High,    FatfListed = true,   RiskDetails = null},
                    new() { Code = "KW", Name = "Kuwait",               RiskLevel = RiskLevel.High,    FatfListed = true,   RiskDetails = null},
                    
                    // Critical Risk
                    new() { Code = "NG", Name = "Nigeria",              RiskLevel = RiskLevel.Critical,    FatfListed = false,  RiskDetails = CountryRiskTag.GamblingAndCasino},
                    new() { Code = "GH", Name = "Ghana",                RiskLevel = RiskLevel.Critical,    FatfListed = false,  RiskDetails = CountryRiskTag.GamblingAndCasino},
                    new() { Code = "IR", Name = "Iran",                 RiskLevel = RiskLevel.Critical,    FatfListed = true,   RiskDetails = null},
                    new() { Code = "KP", Name = "North Korea",          RiskLevel = RiskLevel.Critical,    FatfListed = true,   RiskDetails = null},
                    new() { Code = "MM", Name = "Myanmar",              RiskLevel = RiskLevel.Critical,    FatfListed = true,   RiskDetails = CountryRiskTag.Drug}};
                    
                    db.Countries.AddRange(countries);
            }


            if(!await db.MccCodes.AnyAsync(ct))
            {
                var mccCodes = new List<MccCode>{
                
                    // Low Risk
                    new() { Mcc = "5411", Category = "Grocery Stores/Supermarkets", RiskLevel = RiskLevel.Low },
                    new() { Mcc = "5812", Category = "Eating Places/Restaurants", RiskLevel = RiskLevel.Low },
                    new() { Mcc = "5651", Category = "Family Clothing Stores", RiskLevel = RiskLevel.Low },
                    new() { Mcc = "5912", Category = "Drug Stores/Pharmacies", RiskLevel = RiskLevel.Low },
                    new() { Mcc = "5817", Category = "Digital Goods: Software Applications (Excluding Games)", RiskLevel = RiskLevel.Low },
                    new() { Mcc = "5734", Category = "Computer Software Stores", RiskLevel = RiskLevel.Low },
                    new() { Mcc = "5199", Category = "Nondurable Goods: not elsewhere classified", RiskLevel = RiskLevel.Low },

                    // Medium Risk
                    new() { Mcc = "4900", Category = "Utilities: Electric, Gas, Sanitary, Water", RiskLevel = RiskLevel.Medium },
                    new() { Mcc = "4722", Category = "Travel Agencies and Tour Operators", RiskLevel = RiskLevel.Medium },
                    new() { Mcc = "6513", Category = "Real Estate Agents and Managers: Rentals", RiskLevel = RiskLevel.Medium },
                    new() { Mcc = "4215", Category = "Courier Services: Air and Ground, Freight Forwarders", RiskLevel = RiskLevel.Medium },
                    new() { Mcc = "5815", Category = "Digital Goods: Audiovisual Media Including Books, Movies, and Music", RiskLevel = RiskLevel.Medium },

                    // High Risk
                    new() { Mcc = "5933", Category = "Pawn Shops", RiskLevel = RiskLevel.High },
                    new() { Mcc = "5094", Category = "Precious Stones/Metals/Watches/Jewelry", RiskLevel = RiskLevel.High },
                    new() { Mcc = "8398", Category = "Organizations, Charitable and Social Service", RiskLevel = RiskLevel.High },
                    new() { Mcc = "4829", Category = "Money Transfer", RiskLevel = RiskLevel.High },
                    new() { Mcc = "6050", Category = "Quasi Cash: Customer Financial Institution", RiskLevel = RiskLevel.High },

                    // Critical Risk
                    new() { Mcc = "7995", Category = "Gambling Transactions", RiskLevel = RiskLevel.Critical }

                };

                db.MccCodes.AddRange(mccCodes);
            }

            await db.SaveChangesAsync(ct);


            var countryIdByCode = await db.Countries.AsNoTracking()
                .ToDictionaryAsync(c => c.Code, c => c.Id, ct);

            var mccIdByCode = await db.MccCodes.AsNoTracking()
                .ToDictionaryAsync(m => m.Mcc, m => m.Id, ct);


            if (!await db.Merchants.AnyAsync(ct))
            {
                //null = not CryptoArbitrage (not applicable), true/false = only for CryptoArbitrage
                var merchants = new List<Merchant> { 
                    new() { MccId = mccIdByCode["5411"], Name = "Magnum Cash&Carry",               IsAifc = null,  CountryId = countryIdByCode["KZ"]}, 
                    new() { MccId = mccIdByCode["5812"], Name = "KFC Kazakhstan",                  IsAifc = null,  CountryId = countryIdByCode["KZ"]},
                    new() { MccId = mccIdByCode["5812"], Name = "Global Coffee Company",           IsAifc = null,  CountryId = countryIdByCode["KZ"]},
                    new() { MccId = mccIdByCode["5812"], Name = "Wok Wok by Meyram Group",         IsAifc = null,  CountryId = countryIdByCode["KZ"]},
                    new() { MccId = mccIdByCode["5817"], Name = "Anthropic, PBC",                  IsAifc = null,  CountryId = countryIdByCode["US"]},
                    new() { MccId = mccIdByCode["5817"], Name = "YouTube Red",                     IsAifc = null,  CountryId = countryIdByCode["US"]},
                    new() { MccId = mccIdByCode["4900"], Name = "Elorda Eco System",               IsAifc = null,  CountryId = countryIdByCode["KZ"]}, 
                    new() { MccId = mccIdByCode["4722"], Name = "Airbnb",                          IsAifc = null,  CountryId = countryIdByCode["US"]}, 
                    new() { MccId = mccIdByCode["4829"], Name = "Western Union",                   IsAifc = null,  CountryId = countryIdByCode["US"]}, 
                    new() { MccId = mccIdByCode["5094"], Name = "Dubai Gold Exchange",             IsAifc = null,  CountryId = countryIdByCode["AE"]}, 
                    new() { MccId = mccIdByCode["7995"], Name = "Olympic Casino",                  IsAifc = null,  CountryId = countryIdByCode["KZ"]},
                    new() { MccId = mccIdByCode["6050"], Name = "AIFC FinTech Pay Ltd",            IsAifc = true,  CountryId = countryIdByCode["KZ"]},
                    new() { MccId = mccIdByCode["6050"], Name = "ByBit",                           IsAifc = false, CountryId = countryIdByCode["SC"]},
                    new() { MccId = mccIdByCode["7995"], Name = "FIX PLAY",                        IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["5094"], Name = "Mandalay Ruby & Gems Co.",        IsAifc = null,  CountryId = countryIdByCode["MM"]},
                    new() { MccId = mccIdByCode["5812"], Name = "Luna Restaurant",                 IsAifc = null,  CountryId = countryIdByCode["RU"]},
                    new() { MccId = mccIdByCode["4829"], Name = "Kabul Exchange & Money Transfer", IsAifc = null,  CountryId = countryIdByCode["AF"]},

                    new() { MccId = mccIdByCode["6050"], Name = "Binance.com", IsAifc = false,  CountryId = countryIdByCode["US"]},
                    new() { MccId = mccIdByCode["6050"], Name = "Binance.kz", IsAifc = true,  CountryId = countryIdByCode["KZ"]},
                    new() { MccId = mccIdByCode["6050"], Name = "BNB", IsAifc = false,  CountryId = countryIdByCode["GB"]},

                    new() { MccId = mccIdByCode["4215"], Name = "Stealth Shift Dynamics", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["4215"], Name = "Zettofee Courier", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["5734"], Name = "Nextech Cyber Emporium", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["5734"], Name = "Awattade Ventures", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["5199"], Name = "Purse & Pixel", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["5199"], Name = "Trend Art Craft", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["5815"], Name = "Zwiftfine E-books", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["7995"], Name = "SKFN", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["7995"], Name = "sklarc", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["7995"], Name = "k2p", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["4215"], Name = "Swift Parcel Hub", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["5734"], Name = "OmniTech Store", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["5199"], Name = "Vogue & Tote", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["5815"], Name = "PixelRead Media", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["7995"], Name = "X8BET", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["7995"], Name = "pxp9", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["7995"], Name = "SPIN MAX", IsAifc = null,  CountryId = countryIdByCode["NG"]},
                    new() { MccId = mccIdByCode["5734"], Name = "Zenith Logic Solutions", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["5199"], Name = "Velora Craft", IsAifc = null,  CountryId = countryIdByCode["GH"]},
                    new() { MccId = mccIdByCode["7995"], Name = "zpay", IsAifc = null,  CountryId = countryIdByCode["GH"]},   
                    
                    
                    new() { MccId = mccIdByCode["5933"], Name = "Lombard Astana", IsAifc = null,  CountryId = countryIdByCode["KZ"]},   
                   
                    new() { MccId = mccIdByCode["5651"], Name = "Almaty Outfit House", IsAifc = null,  CountryId = countryIdByCode["KZ"]},   
                    new() { MccId = mccIdByCode["5912"], Name = "Europharma Kazakhstan", IsAifc = null,  CountryId = countryIdByCode["KZ"]},   
                    new() { MccId = mccIdByCode["6513"], Name = "Nomad Housing Exchange", IsAifc = null,  CountryId = countryIdByCode["KZ"]},   
                    new() { MccId = mccIdByCode["8398"], Name = "Red Crescent Kazakhstan", IsAifc = null,  CountryId = countryIdByCode["KZ"]}   
                };

                db.Merchants.AddRange(merchants);
            }


            if (!await db.ExchangeRates.AnyAsync(ct))
            {
                var exchangeRates = new List<ExchangeRates> {
                    new() { Currency = "USD", RateToKzt = 540.00m, RateDate = SeedDate},
                    new() { Currency = "EUR", RateToKzt = 580.00m, RateDate = SeedDate},
                    new() { Currency = "RUB", RateToKzt = 5.50m,   RateDate = SeedDate},
                    new() { Currency = "KZT", RateToKzt = 1m,      RateDate = SeedDate}

                };

                db.ExchangeRates.AddRange(exchangeRates);
            }


            if(!await db.Rules.AnyAsync(ct))
            {
                var rules = new List<Rule> {
                    new() { Type = RuleType.Hard, 
                            Action = RuleAction.Block,
                            RuleCode = "CRITICAL_RISK_COUNTRY", 
                            Details = "The operation is associated with a Country with a Critical risk level (the country of the operation, counterparty, or merchant)", 
                            ParametersJson = "{}",  
                            Weight = 0, 
                            IsActive = true, 
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Hard,
                            Action = RuleAction.Block,
                            RuleCode = "CRITICAL_RISK_MERCHANT",
                            Details = "The operation is associated with a Merchant with a Critical risk level",
                            ParametersJson = "{}",
                            Weight = 0,
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Typology,
                            Action = RuleAction.Block,
                            RuleCode = "CRYPTO",
                            Details = "Crypto arbitrage behaviour is detected",
                            ParametersJson = JsonSerializer.Serialize(new {
                            typologyConfidenceScoreThreshold = ScreeningThresholds.TypologyConfidenceScoreThreshold,
                            unevenAmountRatioMinRange =  CryptoArbitrageThresholds.UnevenAmountRatioMinRange,
                            transactionsForRatioCheckMinRange = CryptoArbitrageThresholds.TransactionsForRatioCheckMinRange,
                            p2pAndIntrabankCountLast30DaysMinRange = CryptoArbitrageThresholds.P2pAndIntrabankCountLast30DaysMinRange,
                            uniqueCounterpartyCountLast30DaysMinRange = CryptoArbitrageThresholds.UniqueCounterpartyCountLast30DaysMinRange,
                            nonResidentCounterpartyCountLast30DaysMinRange = CryptoArbitrageThresholds.NonResidentCounterpartyCountLast30DaysMinRange,
                            nonAifcCryptoExchangeTransactionCountLast30DaysMinRange = CryptoArbitrageThresholds.NonAifcCryptoExchangeTransactionCountLast30DaysMinRange}),
                            Weight = 0, 
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Typology,
                            Action = RuleAction.Block,
                            RuleCode = "DROPPER",
                            Details = "Dropper behaviour is detected",
                            ParametersJson = JsonSerializer.Serialize(new {
                            typologyConfidenceScoreThreshold = ScreeningThresholds.TypologyConfidenceScoreThreshold,
                            clientAgeDaysMinRange = DropperThresholds.ClientAgeDaysMinRange,
                            retailTransactionCountLast30DaysMinRange = DropperThresholds.RetailTransactionCountLast30DaysMinRange,
                            balancePercentageMinRange = DropperThresholds.BalancePercentageMinRange,
                            minutesToFirstWithdrawalAfterIncomingMaxRange = DropperThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange,
                            p2pAmountsLast24hMinRange = DropperThresholds.P2pAmountsLast24hMinRange,
                            p2pCountMinRange = DropperThresholds.P2pCountMinRange,
                            uniqueCounterpartiesLast24hMinRange = DropperThresholds.UniqueCounterpartiesLast24hMinRange,
                            counterpartiesInAntifraudListMinRange = DropperThresholds.CounterpartiesInAntifraudListMinRange}),
                            Weight = 0,
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Typology,
                            Action = RuleAction.Block,
                            RuleCode = "DRUG",
                            Details = "Drug behaviour is detected",
                            ParametersJson = JsonSerializer.Serialize(new {
                            typologyConfidenceScoreThreshold = ScreeningThresholds.TypologyConfidenceScoreThreshold,
                            clientAgeDaysMinRange = DrugThresholds.ClientAgeDaysMinRange,
                            clientAgeMinRange = DrugThresholds.ClientAgeMinRange,
                            clientAgeMaxRange = DrugThresholds.ClientAgeMaxRange,
                            uniqueIncomingCounterpartiesLast24hMinRange = DrugThresholds.UniqueIncomingCounterpartiesLast24hMinRange,
                            uniqueIncomingCardCountLast24hMinRange = DrugThresholds.UniqueIncomingCardCountLast24hMinRange,
                            incomingP2pCountLast24hMinRange = DrugThresholds.IncomingP2pCountLast24hMinRange,
                            drugPriceMinRange = DrugThresholds.DrugPriceMinRange,
                            drugPriceMaxRange = DrugThresholds.DrugPriceMaxRange,
                            balancePercentageMinRange = DrugThresholds.BalancePercentageMinRange,
                            minutesToFirstWithdrawalAfterIncomingMaxRange = DrugThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange,
                            incomingAndOutcomingP2pCount3dMinRange = DrugThresholds.IncomingAndOutcomingP2pCount3dMinRange}),
                            Weight = 0,
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Typology,
                            Action = RuleAction.Block,
                            RuleCode = "GAMBLING",
                            Details = "Gambling behaviour is detected",
                            ParametersJson = JsonSerializer.Serialize(new {
                            typologyConfidenceScoreThreshold = ScreeningThresholds.TypologyConfidenceScoreThreshold,
                            merchantCategoryCountLast24hMinRange = GamblingThresholds.MerchantCategoryCountLast24hMinRange,
                            incomingFromGamblingMerchantsCountLast24hMinRange = GamblingThresholds.IncomingFromGamblingMerchantsCountLast24hMinRange,
                            retailTransactionCountLast30DaysMinRange = GamblingThresholds.RetailTransactionCountLast30DaysMinRange,
                            doubledAmountsTolerance = GamblingThresholds.DoubledAmountsTolerance,
                            uniqueForeignMerchantTransactionCountLast24hMinRange = GamblingThresholds.UniqueForeignMerchantTransactionCountLast24hMinRange,
                            minutesDifferenceBetweenTransactionsMaxRange = GamblingThresholds.MinutesDifferenceBetweenTransactionsMaxRange,
                            fractionalAmountCountMinRange = GamblingThresholds.FractionalAmountCountMinRange}),
                            Weight = 0,
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Scoring,
                            Action = RuleAction.Flag,
                            RuleCode = "HIGH_FREQUENCY",
                            Details = "High frequency behaviour is detected",
                            ParametersJson = JsonSerializer.Serialize(new { transactionCountLastHourMinRange = CriticalAndHighRiskThresholds.TransactionCountLastHourMinRange }),
                            Weight = CriticalAndHighRiskThresholds.HighFrequencyScoringScoreThreshold,
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Scoring,
                            Action = RuleAction.Block,
                            RuleCode = "HIGH_RISK_COUNTRY",
                            Details = "The operation is associated with a Country with a High risk level (the country of the operation, counterparty, or merchant)",
                            ParametersJson = JsonSerializer.Serialize(new { highRiskCountryTransactions3dMinRange = CriticalAndHighRiskThresholds.HighRiskCountryTransactions3dMinRange }),
                            Weight = ScreeningThresholds.ScoringScoreThreshold,
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Scoring,
                            Action = RuleAction.Flag,
                            RuleCode = "HIGH_RISK_MERCHANT",
                            Details = "The operation is associated with a Merchant with a High risk level",
                            ParametersJson = JsonSerializer.Serialize(new { highRiskMerchantCountLastHourMinRange = CriticalAndHighRiskThresholds.HighRiskMerchantCountLastHourMinRange}),
                            Weight = CriticalAndHighRiskThresholds.HighRiskMerchantScoringScoreThreshold,
                            IsActive = true,
                            CreatedAt = SeedDate},

                    new() { Type = RuleType.Hard,
                            Action = RuleAction.Block,
                            RuleCode = "WATCHLIST",
                            Details = "The client is detected in one of the black lists",
                            ParametersJson = "{}",
                            Weight = 0,
                            IsActive = true,
                            CreatedAt = SeedDate},
                };

                db.Rules.AddRange(rules);
            }


            if(!await db.KnownCounterparties.AnyAsync(ct))
            {
                var counterparties = new List<KnownCounterparty> {
                    
                    // Antifraud
                    KnownCounterparty.Create("Maxim A.B.", CounterpartyType.AntifraudListed, "AFC", SeedDate),
                    KnownCounterparty.Create("Milana L.N.", CounterpartyType.AntifraudListed, "AFC", SeedDate),
                    KnownCounterparty.Create("Takhir R.A.", CounterpartyType.AntifraudListed, "AFC", SeedDate),
                    KnownCounterparty.Create("Aidana B.B.", CounterpartyType.AntifraudListed, "AFC", SeedDate),

                    //Gambling
                    KnownCounterparty.Create("Zettofee Courier", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Nextech Cyber Emporium", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Awattade Ventures", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Purse & Pixel", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Trend Art Craft", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Zwiftfine E-books", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("SKFN", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("sklarc", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("k2p", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("FIX PLAY", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Swift Parcel Hub", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("OmniTech Store", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Vogue & Tote", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("PixelRead Media", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("X8BET", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("pxp9", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("SPIN MAX", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Zenith Logic Solutions", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Velora Craft", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("zpay", CounterpartyType.Gambling, "Internal Blacklist", SeedDate),
                    KnownCounterparty.Create("Stealth Shift Dynamics", CounterpartyType.Gambling, "Internal Blacklist", SeedDate)
                };

                db.KnownCounterparties.AddRange(counterparties);

            }


            await db.SaveChangesAsync(ct);

           
        }
    }
}

