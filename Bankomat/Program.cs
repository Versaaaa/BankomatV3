using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bankomat
{
    internal class Program
    {
        static void Main(string[] args)
        {

            LoginUi loginUi = new LoginUi();
            
            if(loginUi.Login()) 
            { 
                using (var ctx = new BankomatV2Entities())
                {
                    BankUI ui = new BankUI(ctx.Clienti.Where(x => x.Cf.Equals(loginUi.Cf)).Include(x => x.ContiCorrente).Include(x => x.ContiCorrente.Banche).First());
                    ui.MainMenu();
                    ctx.SaveChanges();
                }            
            }
        }
    }

    public partial class ContiCorrente
    {
        public void Versamento(double importValue)
        {
            Saldo += importValue;
            Saldo = Math.Round(Saldo, 4, MidpointRounding.AwayFromZero);
            DataUltimoVersamento = DateTime.Now;
        }

        public void Prelievo(double quantita)
        {
            Saldo -= quantita;
            Saldo = Math.Round(Saldo, 4, MidpointRounding.AwayFromZero);
        }

    }

    public partial class Clienti
    {
        public int Tentativi { get; set; } = 0;
        public int TentativiMassimi { get; set; } = 3;
    }

    public static class UiHelper
    {
        public static void PrintMenuHeader(string menuTitle)
        {
            Console.Clear();
            Console.WriteLine("**************************************************");
            Console.WriteLine("*              Bankomat Simulator               *");
            Console.WriteLine("**************************************************");
            Console.WriteLine("".PadLeft((50 - menuTitle.Length) / 2) + menuTitle);
            Console.WriteLine("--------------------------------------------------");
        }
    }

    public interface IBankFunctionHandler
    {
        void Versamento();
        void ReportSaldo();
        void Prelievo();
        void RegistroOperazioni();
    }

    public class BankFunctionHandler : IBankFunctionHandler
    {

        ContiCorrente _selectedContoCorrente;

        public BankFunctionHandler(ContiCorrente selectedContoCorrente)
        {
            _selectedContoCorrente = selectedContoCorrente;
        }

        public void Prelievo()
        {
            Console.Write("Inserisci l'importo da prelevare: ");

            var import = Console.ReadLine();

            if (!double.TryParse(import, out double importValue))
            {
                Console.WriteLine("Operazione annullata - Inserire un numero");
                Console.Write("Premere un tasto per proseguire");
                Console.ReadKey();
                return;
            }

            if (importValue < 0)
            {
                Console.WriteLine("Operazione annullata - Inserire un numero positivo");
                Console.Write("Premere un tasto per proseguire");
                Console.ReadKey();
                return;
            }

            if (_selectedContoCorrente.Saldo < importValue)
            {
                Console.WriteLine($"Operazione annullata - Saldo non sufficiente");
            }
            else
            {
                _selectedContoCorrente.Prelievo(importValue);
                Console.WriteLine($"Operazione completata - Importo prelevato: {importValue} ");
                Console.WriteLine($"                      - Saldo Rimanente: {_selectedContoCorrente.Saldo} ");

                _selectedContoCorrente.Movimenti.Add(new Movimenti()
                {
                    Ammontare = (int)importValue,
                    ContiCorrente = _selectedContoCorrente,
                    Data = DateTime.Now,
                    Operazione = "-"
                });
            }


            Console.Write("Premere un tasto per proseguire");
            Console.ReadKey();
        }

        // not implemented
        public void RegistroOperazioni()
        {
            foreach (var i in _selectedContoCorrente.Movimenti)
            {
                Console.WriteLine();
                Console.WriteLine($"Operazione: {(i.Operazione == "-" ? "Prelievo" : "Versamento")}");
                Console.WriteLine($"Ammontare: {i.Ammontare}");
                Console.WriteLine($"Data / ora : {i.Data}");
            }
            Console.Write("Premere un tasto per proseguire");
            Console.ReadKey();
        }

        public void ReportSaldo()
        {
            Console.WriteLine($"Saldo conto: {_selectedContoCorrente.Saldo}");
            Console.WriteLine($"Data ultimo versamento: {_selectedContoCorrente.DataUltimoVersamento}");
            Console.WriteLine($"Data / ora attuale: {DateTime.Now}");

            Console.Write("Premere un tasto per proseguire");
            Console.ReadKey();
        }

        public void Versamento()
        {
            Console.Write("Inserisci l'importo del versamento: ");

            var import = Console.ReadLine();
            if (!double.TryParse(import, out double importValue))
            {
                Console.WriteLine("Operazione annullata - Inserire un numero");
                Console.Write("Premere un tasto per proseguire");
                Console.ReadKey();
                return;
            }

            if (importValue < 0)
            {
                Console.WriteLine("Operazione annullata - Inserire un numero positivo");
                Console.Write("Premere un tasto per proseguire");
                Console.ReadKey();
                return;
            }

            _selectedContoCorrente.Versamento(importValue);

            _selectedContoCorrente.Movimenti.Add(new Movimenti()
            {
                Ammontare = (int)importValue,
                ContiCorrente = _selectedContoCorrente,
                Data = DateTime.Now,
                Operazione = "+"
            });
            Console.WriteLine($"Operazione completata - Conteggio saldo: {_selectedContoCorrente.Saldo}");
            Console.Write("Premere un tasto per proseguire");
            Console.ReadKey();
        }
    }

    public class FeatureHandler
    {
        public enum Feature
        {
            Versamento = 0,
            ReportSaldo = 1,
            Prelievo = 2,
            RegistroOperazioni = 3,
        }

        public List<Action> Features { get; set; }
        public List<string> FeatureNames { get; set; }

        public FeatureHandler(List<Funzioni> features, IBankFunctionHandler bankFunctionHandler) 
        {

            Features = new List<Action>() 
            { 
                bankFunctionHandler.Versamento
            };

            FeatureNames = new List<string>() 
            { 
                "Versamento"
            };

            foreach (var feature in features)
            {
                if((Feature)feature.Id == Feature.ReportSaldo)
                {
                    Features.Add(bankFunctionHandler.ReportSaldo);
                }
                else if((Feature)feature.Id == Feature.Prelievo)
                {
                    Features.Add(bankFunctionHandler.Prelievo);
                }
                else if((Feature)feature.Id == Feature.RegistroOperazioni)
                {
                    Features.Add(bankFunctionHandler.RegistroOperazioni);
                }
                else
                {
                    throw new NotImplementedException("Feature not handled");
                }
                FeatureNames.Add(Enum.GetName(typeof(Feature), feature.Id));
            }

        }
    }

    public class LoginUi 
    {
        public string Cf { get; set; }

        public bool Login()
        {
            using (var ctx = new BankomatV2Entities())
            {
                while (true)
                {
                    UiHelper.PrintMenuHeader($"Selezione Banca");
                    {

                        Console.WriteLine($"0 - Esci");

                        foreach (var banca in ctx.Banche.OrderBy(x => x.Id))
                        {
                            Console.WriteLine($"{banca.Id} - {banca.Nome}");
                        } 

                        var input = Console.ReadLine();
                        int inputNum = -1;
                        
                        try
                        {
                            inputNum = int.Parse(input);
                            var res = ValidateLogin(ctx.Banche.First(x => x.Id == inputNum), out var outputMessage);
                            UiHelper.PrintMenuHeader($"{outputMessage}");
                            Console.WriteLine("Premere un tasto per proseguire");
                            Console.ReadLine();
                            if (res != null)
                            {
                                Cf = res;
                                return true;
                            }
                        }
                        catch (FormatException)
                        {
                            UiHelper.PrintMenuHeader($"Errore - Inserire un numero");
                            Console.Write("Premere un tasto per proseguire");
                            Console.ReadLine();
                        }
                        catch (InvalidOperationException)
                        {
                            if(inputNum == 0)
                            {
                                return false;
                            }
                            UiHelper.PrintMenuHeader($"Errore - Indice banca non trovato");
                            Console.Write("Premere un tasto per proseguire");
                            Console.ReadLine();
                        }
                    }
                }
            }
        }

        public string ValidateLogin(Banche selectedBank, out string outputMessage)
        {
            UiHelper.PrintMenuHeader($"Login - {selectedBank.Nome}");

            Console.Write("Nome utente: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            using (var ctx = new BankomatV2Entities())
            {
                var user = ctx.Clienti.FirstOrDefault(x => x.Username == username && x.ContiCorrente.IdBanca == selectedBank.Id);
                if (user == null)
                {
                    outputMessage = "Utente o password errati";
                    return null;
                }
                if (user.Bloccato)
                {
                    outputMessage = "*** Account utente bloccato ***";
                    return null;
                }
                if (user.Password != password)
                {
                    user.Tentativi++;
                    outputMessage = $"Password errata - Tentativi residui: {user.TentativiMassimi - user.Tentativi}";
                    if (user.Tentativi >= user.TentativiMassimi)
                    {
                        user.Bloccato = true;
                        ctx.SaveChanges();
                    }
                    return null;
                }
                outputMessage = "Login effettuato con successo";
                return user.Cf;

            }

        }
    }

    public class BankUI
    {
        Banche _selectedBank;
        ContiCorrente _selectedContiCorrente;
        FeatureHandler _featureHandler;

        public BankUI(Clienti selectedUser)
        {
            _selectedContiCorrente = selectedUser.ContiCorrente;
            _selectedBank = _selectedContiCorrente.Banche;

            _featureHandler = new FeatureHandler(_selectedBank.Funzioni.OrderBy(x => x.Id).ToList(), new BankFunctionHandler(_selectedContiCorrente));
        }

        public void MainMenu()
        {
            while (true)
            {

                UiHelper.PrintMenuHeader($"Menu principale - {_selectedBank.Nome}");

                {
                    var i = 0;
                    Console.WriteLine($"{i} - Esci");
                    foreach (var function in _featureHandler.FeatureNames)
                    {
                        i++;
                        Console.WriteLine($"{i} - {function}");
                    }
                }

                var input = Console.ReadLine();
                int inputNum = -10;

                try
                {
                    inputNum = int.Parse(input) - 1;
                    UiHelper.PrintMenuHeader($"{_featureHandler.FeatureNames[inputNum]} - {_selectedBank.Nome}");
                    _featureHandler.Features[inputNum].Invoke();
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (inputNum == -1)
                    {
                        return;
                    }
                    UiHelper.PrintMenuHeader($"Errore - Comando inserito non esistente");
                    Console.Write("Premere un tasto per proseguire");
                    Console.ReadLine();
                } 
                catch (System.FormatException)
                {
                    UiHelper.PrintMenuHeader($"Errore - Inserire un numero");
                    Console.Write("Premere un tasto per proseguire");
                    Console.ReadLine();
                }
            }
        }
    }
}
