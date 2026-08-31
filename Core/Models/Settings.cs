using Core.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Env
    {
        public static string Development = "Development";
        public static string Staging = "Staging";
        public static string Production = "Production";
    }

    public class ServerParams
    {
        public string BackendProtocol { get; set; }
        public string BackendHost { get; set; }
        public int BackendPort { get; set; }
        public string BackendPrefix { get; set; }

        public override string ToString()
        {
            return String.Format("{0}://{1}:{2}{3}", BackendProtocol, BackendHost, BackendPort, BackendPrefix);
        }
    }


    public class Settings : INotifyPropertyChanged
    {
        // Singleton Instance
        public static Settings Current { get; } = new Settings();

        private readonly SqliteSettingsRepository _repo;

        private string _environment;
        public string Environment
        {
            get => _environment;
            set
            {
                if (_environment != value)
                {
                    Trace.WriteLine("[ Settings ] Environment changed from " + _environment + " to " + value);
                    _environment = value;
                    OnPropertyChanged();

                    // Trigger the async save without blocking the UI thread
                    // The '_' is a discard, telling the compiler we intentionally aren't awaiting this Task here.
                    _ = _repo.SaveSettingAsync("environment", value);

                    switch (value)
                    {
                        case var env when env == Env.Development:
                            ServerParams = _defaultDevServerParams;
                            break;
                        case var env when env == Env.Staging:
                            ServerParams = _defaultStagingServerParams;
                            break;
                        case var env when env == Env.Production:
                            ServerParams = _defaultProdServerParams;
                            break;
                        default:
                            string message = "[ Settings ] Warning: Unrecognized wsl" +
                                "environment value: " + value;
                            Trace.WriteLine(message);
                            throw new Exception(message);
                    }

                }
            }
        }

        private ServerParams _defaultDevServerParams = new ServerParams()
        {
            BackendProtocol = "http",
            BackendHost = "localhost",
            BackendPort = 8000,
            BackendPrefix = ""
        };
        private ServerParams _defaultStagingServerParams = new ServerParams()
        {
            BackendProtocol = "https",
            BackendHost = "testlsuadhd.centralus.cloudapp.azure.com",
            BackendPort = 443,
            BackendPrefix = "/api"
        };
        private ServerParams _defaultProdServerParams = new ServerParams()
        {
            BackendProtocol = "https",
            BackendHost = "lsuadhd.centralus.cloudapp.azure.com",
            BackendPort = 443,
            BackendPrefix = "/api"
        };

        private ServerParams _serverParams;
        public ServerParams ServerParams
        {
            get => _serverParams;
            set
            {
                if (_serverParams != value)
                {
                    _serverParams = value;
                    OnPropertyChanged();
                    _repo.SaveSettingSync("backend_protocol", value.BackendProtocol);
                    _repo.SaveSettingSync("backend_host", value.BackendHost);
                    _repo.SaveSettingSync("backend_port", value.BackendPort.ToString());
                    _repo.SaveSettingSync("backend_prefix", value.BackendPrefix);
                }
            }
        }

        private bool _useRealSessionExecutionService;
        public bool UseRealSessionExecutionService
        {
            get => _useRealSessionExecutionService;
            set
            {
                _useRealSessionExecutionService = value;
                _repo.SaveSettingSync("use_real_session_execution_service", value.ToString());
            }
        }

        private Settings()
        {
            Debug.WriteLine("[ Settings ] Constructor called");
            _repo = new SqliteSettingsRepository();

            // Load synchronously on first access to ensure UI has the value immediately
            string? env = _repo.LoadSettingSync("environment");
            if (env == null)
            {
                _environment = Env.Production; // Default value if not set in DB
                _serverParams = _defaultProdServerParams;
                _repo.SaveSettingSync("environment", _environment);
                _repo.SaveSettingSync("backend_protocol", _serverParams.BackendProtocol);
                _repo.SaveSettingSync("backend_host", _serverParams.BackendHost);
                _repo.SaveSettingSync("backend_port", _serverParams.BackendPort.ToString());
                _repo.SaveSettingSync("backend_prefix", _serverParams.BackendPrefix);
            }
            else
            {
                _environment = env;
                string? backendProtocol = _repo.LoadSettingSync("backend_protocol");
                string? backendHost = _repo.LoadSettingSync("backend_host");
                string? backendPortStr = _repo.LoadSettingSync("backend_port");
                string? backendPrefix = _repo.LoadSettingSync("backend_prefix");
                if (backendProtocol == null || backendHost == null || backendPortStr == null || backendPrefix == null)
                    throw new Exception("[ Settings ] Loaded environment from Database but the values for the ServerParams are null");
                int backendPort = int.Parse(backendPortStr);
                _serverParams = new ServerParams()
                {
                    BackendProtocol = backendProtocol,
                    BackendHost = backendHost,
                    BackendPort = backendPort,
                    BackendPrefix = backendPrefix
                };
            }

            var val = _repo.LoadSettingSync("use_real_session_execution_service");
            if (val == null)
                UseRealSessionExecutionService = false;
            else
                _useRealSessionExecutionService = bool.Parse(val);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
