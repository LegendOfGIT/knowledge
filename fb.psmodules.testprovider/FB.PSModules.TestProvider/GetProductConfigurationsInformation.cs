using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace FB.PSModules.UnitTestProvider
{
    [System.Management.Automation.Cmdlet(System.Management.Automation.VerbsCommon.Get, "ProductConfigurationsInformation")]
    public class GetProductConfigurationsInformationCmdlet : PSCmdlet
    {
        private static Encoding Encoding { get { return Encoding.GetEncoding(1252); } }

        const string StageControlcenterReleaseCandidate = "ccreleasecandidate";
        const string StageControlcenterLive = "cclive";

        public static IEnumerable<TestProviderResponseItem> GetProductconfigurationsInformation(string stage, string calculationCoreId, string tarifId)
        {
            var productconfigurations = new List<TestProviderResponseItem>();

            stage = stage ?? string.Empty;
            stage = stage.ToLower();

            using (var connection = GetConnection(stage))
            {
                var command = default(MySqlCommand);                
                var datareader = default(MySqlDataReader);
                var databasecontent = default(string);
                var query = default(StringBuilder);

                //  Mapping Stage >> Datenbankbenutzer
                var stageuser = new Dictionary<string, string>{
                    { "alpha", "3" }, { "beta", "3" },
                    { "releasecandidate", "2" }, { "bialloreleasecandidate", "2" }, { StageControlcenterReleaseCandidate, "2" },
                    { "live", "1" }, { "biallolive", "1" }, { StageControlcenterLive, "1" },
                };

                //  Abfragen der aktiven Produktkonfigurations-IDs
                query = new StringBuilder();
                //  Selektiere
                query.AppendLine("SELECT ");
                //  ... ProduktkonfigurationsID, Benutzer und ProduktID
                query.AppendLine("pkfg.pkfg_id, reke.preke_fid_benutzer ");
                //  ... aus
                query.AppendLine("FROM ");
                //  ... der Rechenkern- und PKFG-Tabelle
                query.AppendLine("tbl_n_p_p_reke reke, pkfg_produkt_nm pkfg, tbl_n_p_p p ");
                //  ... mit dem Filter
                query.AppendLine("WHERE ");
                //  ... RechenkernID = calculationCoreId
                query.AppendLine(string.Format("reke.preke_rkid = '{0}' AND ", calculationCoreId ?? string.Empty));

                //  Einschränkung auf eine Tarif-ID, falls diese angegeben wurde
                if (!string.IsNullOrEmpty(tarifId))
                { 
                    //  ... TarifID = tarifId
                    query.AppendLine(string.Format("reke.preke_rktarifid = '{0}' AND ", tarifId ?? string.Empty));
                }

                //  ... Produktverbindung zwischen Rechenkerntabelle und PKFG-Tabelle
                query.AppendLine("reke.preke_fid_p_id = pkfg.produkt_id AND ");
                query.AppendLine("reke.preke_fid_p_v = pkfg.produkt_version AND ");
                //  ... nur Hauptprodukte
                query.AppendLine("reke.preke_fid_p_id = p.p_id AND ");
                query.AppendLine("reke.preke_fid_p_v = p.p_v AND ");
                query.AppendLine("p.p_option = 0 AND ");
                //  ... Nur aktuelle ProduktkonfigurationsIDs
                query.AppendLine("pkfg.pkfg_aktuell = 1 AND ");
                //  ... mit aktiven Rechenkernverknüpfungen
                query.AppendLine("reke.preke_aktiv = 1");

                //  Eigentliches Ausführen der Query
                try
                {
                    using (command = connection.CreateCommand()) { 
                        command.CommandText = query.ToString();
                        using (datareader = command.ExecuteReader()) { 
                            //  Auslesen der ProduktkonfigurationsIDs
                            //  Keine Abfrageergebnisse >> Abbruch
                            if (!datareader.HasRows) { 
                                throw new InvalidOperationException(string.Format(
                                    "Für die Rechenkern-ID '{0}' {1} konnten keine Produktkonfigurations-IDs gefunden werden.",
                                    (calculationCoreId ?? string.Empty),
                                    string.IsNullOrEmpty(tarifId) ? string.Empty : string.Format("und die Tarif-ID '{0}'", tarifId)
                                )); 
                            }

                            //  Auswerten der Abfrageergebnisse
                            var productconfigurationid = default(long);
                            var user = stageuser.ContainsKey(stage) ? stageuser[stage] : null;

                            //  Es konnte kein Datenbankbenutzer für das angegebene Stage gefunden werden >> Abbruch
                            if (user == null)
                            {
                                throw new InvalidOperationException("Für das angegebene Stage konnte kein Datenbankbenutzer gefunden werden.");
                            }
                            //  Durchlaufe alle Abfrageergebnisse
                            while (datareader.Read())
                            {
                                //  Stimmt der zugehörige Benutzer mit dem Benutzer des Stages überein?
                                databasecontent = datareader.GetString("preke_fid_benutzer") ?? string.Empty;
                                if (databasecontent == user)
                                {
                                    //  Kann die Produktkonfigurations-ID in der Datenbank in ein long übersetzt werden?
                                    productconfigurationid = datareader.GetInt64("pkfg_id");
                                    if (productconfigurationid > 0)
                                    {
                                        productconfigurations.Add(new TestProviderResponseItem
                                        {
                                            Productconfiguration = productconfigurationid
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                catch (MySqlException mysql) { throw mysql; }
                catch (Exception exception) { throw exception; }

                //  Ergänzen der Testsuites / Testcases
                productconfigurations = GetProductconfigurationsInformation(productconfigurations, stage, connection).ToList();
            }

            return productconfigurations;
        }
        public static IEnumerable<TestProviderResponseItem> GetProductconfigurationsInformation(IEnumerable<TestProviderResponseItem> configurations, string stage, MySqlConnection connection = null)
        {
            if (configurations != null)
            {
                //  Ergänzen der ProduktIDs
                configurations = GetProductIDs(configurations, stage, connection);

                using (connection = GetConnection(stage, connection))
                {
                    using (var command = connection.CreateCommand())
                    {
                        var query = new StringBuilder();

                        //  Selektiere
                        query.AppendLine("SELECT");
                        //  ... die ProduktID und den Namen der Produktteilgruppe
                        query.AppendLine("nmptg.nm_fid_p, ptg.ptg_name");
                        //  ... aus
                        query.AppendLine("FROM");
                        //  ... der Produktteilegruppe Tabelle (tbl_n_p_ptg) und der Produktteile-Produkte Tabelle (tbl_n_p_nm_p_ptg nmptg)
                        query.AppendLine("tbl_n_p_ptg ptg, tbl_n_p_nm_p_ptg nmptg ");
                        query.AppendLine("WHERE");
                        //  ... mit passender Produktzuordnung
                        query.AppendLine(string.Format(
                            "nmptg.nm_fid_p in ({0}) AND ",
                            string.Join(",", configurations.Select(item => item.ProductID))
                        ));
                        query.AppendLine("nmptg.nm_fid_ptg = ptg.ptg_id");

                        command.CommandText = query.ToString();
                        using (var datareader = command.ExecuteReader())
                        {
                            //  Auslesen der ProduktkonfigurationsIDs
                            //  Keine Abfrageergebnisse >> Abbruch
                            if (!datareader.HasRows)
                            {
                                throw new InvalidOperationException("Für die Ermittelten Produkte konnten keine Produktteilegruppen ermittelt werden.");
                            }

                            //  Mapping zwischen Produktteilegruppen und Testfällen
                            var powershellfolder = Environment.GetEnvironmentVariable("Services.Implementationtest.PowershellFolder") ?? string.Empty;
                            var mappingfile = Path.Combine(powershellfolder, "TestProvider", "testfallmapping.csv");
                            var mappinglines = File.Exists(mappingfile) ? File.ReadAllLines(mappingfile, Encoding).Where(line => line.Split(';').Count() > 2) : null;
                            var mapping = mappinglines == null ? null : mappinglines.Select(entry => new KeyValuePair<string, string>(entry.Split(';').First(), entry.Split(';').Skip(1).First()));
                            var teilegruppen_testfaelle = mapping == null ? null : mapping.ToDictionary(entry => entry.Key, entry => entry.Value);

                            while (datareader.Read())
                            {
                                //  Nachfüllen der Testsuiten/cases
                                var teilegruppename = datareader.GetString("ptg_name") ?? string.Empty;
                                if (!string.IsNullOrEmpty(teilegruppename))
                                {
                                    var testfall = teilegruppen_testfaelle.FirstOrDefault(testfallmapping =>
                                        Regex.IsMatch(teilegruppename.ToLower(), testfallmapping.Key)
                                    );
                                    if (!string.IsNullOrEmpty(testfall.Value))
                                    {
                                        var productid = datareader.GetInt64("nm_fid_p");
                                        var productconfigurationitems = configurations.Where(pkfg => pkfg.ProductID == productid);
                                        if (productconfigurationitems != null)
                                        {
                                            foreach (var productconfiguration in productconfigurationitems)
                                            {
                                                productconfiguration.Testsuite = testfall.Value.Split('/')[0];
                                                productconfiguration.Testcase = testfall.Value.Split('/')[1];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return configurations;
        }
        private static IEnumerable<TestProviderResponseItem> GetProductIDs(IEnumerable<TestProviderResponseItem> configurations, string stage, MySqlConnection connection = null)
        {
            var configurationsresult = new List<TestProviderResponseItem>();
            if (configurations != null)
            {
                using (connection = GetConnection(stage, connection))
                {
                    using (var command = connection.CreateCommand())
                    {
                        //  Abfragen der aktiven Produktkonfigurations-IDs
                        var query = new StringBuilder();
                        //  Selektiere
                        query.AppendLine("SELECT DISTINCT ");
                        //  ... ProduktkonfigurationsID und ProduktID
                        query.AppendLine("pkfg.pkfg_id, pkfg.produkt_id ");
                        //  ... aus
                        query.AppendLine("FROM ");
                        //  ... der Rechenkern- und PKFG-Tabelle
                        query.AppendLine("pkfg_produkt_nm pkfg, tbl_n_p_p p ");
                        //  ... mit dem Filter
                        query.AppendLine("WHERE ");
                        //  ... Produktverbindung zwischen Rechenkerntabelle und PKFG-Tabelle
                        query.AppendLine(string.Format(
                            "pkfg.pkfg_id IN ({0}) AND",
                            string.Join(", ", configurations.Select(configuration => configuration.Productconfiguration))
                        ));
                        query.AppendLine("pkfg.produkt_id = p.p_id AND ");
                        query.AppendLine("pkfg.produkt_version = p.p_v AND ");
                        query.AppendLine("p.p_option = 0");

                        command.CommandText = query.ToString();
                        using (var datareader = command.ExecuteReader())
                        {
                            //  Auslesen der ProduktkonfigurationsIDs
                            //  Keine Abfrageergebnisse >> Abbruch
                            if (!datareader.HasRows)
                            {
                                throw new InvalidOperationException("Für die ermittelten Produktkonfigurationen konnten keine ProduktIds ermittelt werden.");
                            }

                            //  Durchlaufe alle Abfrageergebnisse
                            while (datareader.Read())
                            {
                                //  Kann die Produkt-ID in der Datenbank in ein long übersetzt werden?
                                var productid = datareader.GetInt64("produkt_id");
                                if (productid > 0)
                                {
                                    var configuration = configurations.FirstOrDefault(c => c.Productconfiguration == datareader.GetInt64("pkfg_id"));
                                    configurationsresult.Add(new TestProviderResponseItem {
                                        Productconfiguration = configuration.Productconfiguration,
                                        ProductID = productid
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return configurationsresult;
        }
        private static MySqlConnection GetConnection(string stage, MySqlConnection connection = null)
        {
            var key = string.Format(
                "CalculationCoreInformation{0}",

                new[] { StageControlcenterReleaseCandidate, StageControlcenterLive }.Contains(stage ?? string.Empty) ?
                "Controlcenter" :
                string.Empty
            );
            if (connection == null)
            {
                /*
                 * [System.AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", "app.config")
                 * [System.Configuration.ConfigurationManager]::ConnectionStrings["CalculationCoreInforations"].ConnectionString
                 */

                //  Kein ConnectionString vorhanden >> Abbruch
                if (ConfigurationManager.ConnectionStrings[key] == null)
                {
                    throw new ConfigurationErrorsException(string.Format(
                        "Für das Ermitteln der Produktkonfigurationen muss der ConnectionString '{0}' angegeben werden.", key
                    ));
                }
            }

            connection = connection ?? new MySqlConnection(ConfigurationManager.ConnectionStrings[key].ConnectionString);
            if(connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }

        [System.Management.Automation.Parameter(Position = 0, Mandatory = false, HelpMessage = "The stage of the calculation core.", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = true)]
        public string Stage { get; set; }

        [System.Management.Automation.Parameter(Position = 1, Mandatory = false, HelpMessage = "The id of the calculation core.", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = true)]
        public string CalculationCoreId { get; set; }

        [System.Management.Automation.Parameter(Position = 2, Mandatory = false, HelpMessage = "The id of the tarif used by the calculation core.", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = true)]
        public string TarifId { get; set; }

        [System.Management.Automation.Parameter(Position = 3, Mandatory = false, HelpMessage = "Productinformation-ids used to complete configuration information.", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = true)]
        public long[] ConfigurationIds { get; set; }

        protected override void ProcessRecord()
        {
            var result = default(IEnumerable<TestProviderResponseItem>);

            //  Direktangabe von Produktkonfigurations-IDs >> Ergänzen der restlichen Produktkonfigurationsinformationen
            if(this.ConfigurationIds != null && this.ConfigurationIds.Any())
            {
                result = GetProductConfigurationsInformationCmdlet.GetProductconfigurationsInformation(
                    this.ConfigurationIds.Select(configurationid => new TestProviderResponseItem { Productconfiguration = configurationid }),
                    this.Stage
                );
            }
            //  Ansonsten Ermittlung von Produktkonfigurationen über RechenkernID, ggf. TarifID
            else
            {
                result = GetProductConfigurationsInformationCmdlet.GetProductconfigurationsInformation(this.Stage, this.CalculationCoreId, this.TarifId);
            }

            WriteObject(result);
        }
    }
}