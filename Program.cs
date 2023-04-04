using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Net;
using HtmlAgilityPack;

namespace Instalar_mangas
{
    internal class Program
    {
        //URL usada para pesquisa no site
        static string searchURL = "https://mangahosted.com/find/";
        static bool fullDownload = false;
        static HttpClient client = new HttpClient();
        static bool helpUs = true;
        static string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\mangaS";

        //Mensagem padrão de help:
        static string helpMessage = "- Pesquisar mangá digite: mangas nomeDomanga\n- Modo de selecionar capítulos:\n       10  - Instala só o capítulo '10'\n       10-13  - Instala o eles e seus instervalos '10,11,12,13'\n       10,20,30  - Instala apenas os capítulos '10,20,30'\n      Todos  - Instala todos os capítulos\n\nAperte Enter para recomeçar";
        static string versionMessage = "Versão do script: 1.0.0.1";

        static async Task Main(string[] args)
        {
            while (helpUs)
            {
                Console.Clear();
                Console.WriteLine("======== Programa iniciado ========\nDigite help para ver os comandos\n");
                string option = Console.ReadLine();
                option = option.Replace("-", ""); //Caso escreva -help

                switch (option)
                {
                    case "help":
                        Console.Clear();
                        Console.WriteLine(helpMessage);
                        Console.ReadLine();
                        break;
                    case "h":
                        Console.Clear();
                        Console.WriteLine(helpMessage);
                        Console.ReadLine();
                        break;
                    case "version":
                        Console.Clear();
                        Console.WriteLine(versionMessage);
                        Console.ReadLine();
                        break;
                    case "v":
                        Console.Clear();
                        Console.WriteLine(versionMessage);
                        Console.ReadLine();
                        break;
                    default:
                        if (option.Contains("mangas"))
                        {
                            string mangaName = option.Replace("mangas", "");
                            await SearchManga(mangaName);
                            helpUs = false;
                        }
                        break;
                }
            }

            if (fullDownload)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nDownload concluido com sucesso!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nHouve algum durante a instalação.");
            }

            Console.ResetColor();
            Console.ReadLine();
        }

        static async Task SearchManga(string mangaName)
        {
            string mangaNameFolder = mangaName.Trim(); //Tirar espaços inuteis para adicionar + ao campo de pesquisa
            string mangaNameSearch = mangaNameFolder.Replace(" ", "+"); //Colocar + em espaços brancos para a pesquisa
            string chapter = string.Empty;
            bool permission = false;
            bool searchError = false;

            var wc = new HtmlWeb();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  Iniciando busca em Mangahosted.com\n");
            Console.ResetColor();

            var doc = wc.Load(searchURL + mangaNameSearch);
            var mangaGuest = doc.DocumentNode.SelectNodes("//main[@class='box-content box-perfil']/h3");

            if (mangaGuest == null)
            {
                permission = true;
            }
            else
            {
                searchError = true;
            }

            while (permission)
            {
                var cardManga = doc.DocumentNode.SelectNodes("//table[@class='table table-search table-hover']/tbody/tr");
                var cardName = doc.DocumentNode.SelectNodes("//h4[@class='entry-title']/a");

                if (cardManga == null)
                {
                    permission = false;
                    searchError = true;
                }
                else
                {
                    int mangaLength = cardManga.Count - 1;

                    for (int i = 0; i <= mangaLength; i++)
                    {
                        var name = cardName[i].InnerText;
                        var stringLink = cardName[i].OuterHtml;
                        string[] link = stringLink.Split('"');

                        Console.Write("  Você quer abaixar " + name + " [s/n]? ");

                        string namePath = CaracteresNaoPermitidos(name);
                        string choise = Console.ReadLine();

                        switch (choise)
                        {
                            case "s":
                                await SearchChapters(link[1], namePath);
                                permission = false;
                                i = mangaLength; 
                                break;
                            case "S":
                                await SearchChapters(link[1], namePath);
                                permission = false;
                                i = mangaLength;
                                break;
                            default:
                                if (i >= mangaLength)
                                {
                                    permission = false;
                                }
                                break;
                        }
                    }
                }
            }

            if (searchError)
            {

                Console.WriteLine("Manga não encontrado.");
                Console.ReadLine();
            }
        }

        static async Task SearchChapters(string mangaURL, string mangaName)
        {
            var wc = new HtmlWeb();
            var doc = wc.Load(mangaURL);
            bool tryAgain = true;

            var cardChapters = doc.DocumentNode.SelectNodes("//div[@class='chapters']/div");
            var name = doc.DocumentNode.SelectNodes("//div[@class='pop-title']/span[@class='btn-caps']");
            
            if(cardChapters != null && name != null)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n  Capítulos:");

                for (int i = 0; i <= cardChapters.Count - 1; i++)
                {
                    Console.WriteLine("  (" + (i + 1) + ") Capítulo: #" + name[i].InnerText);
                    if (i >= cardChapters.Count - 1)
                    {
                        Console.ResetColor();
                        Console.Write("\nTotal de resultados: " + cardChapters.Count + "\n  Quais capítulos você deseja instalar? ");
                    }
                }
                while(tryAgain)
                {
                    string options = Console.ReadLine();
                    bool executado = true;

                    switch (options)
                    {
                        case "todos":
                            for (int i = 0; i <= cardChapters.Count - 1; i++)
                            {
                                var chapersLinkBruto = doc.DocumentNode.SelectNodes("//div[@class='pop-content']/div[@class='tags']/a");
                                string stringLink = chapersLinkBruto[i].OuterHtml;
                                string link = stringLink.Replace("<a href='", "");
                                link = link.Substring(0, link.IndexOf("'"));
                                string nameCap = name[i].InnerText;
                                await ImgChapter(link, mangaName, nameCap);
                                await Task.Delay(1000); //para não haver erro por estar muito rapido na requisicão

                                if (i >= cardChapters.Count - 1)
                                {
                                    fullDownload = true;
                                    tryAgain = false;
                                }//para finalizar
                            }
                            break;
                        default:
                            if (options.Contains("-"))
                            {
                                options = options.Trim();
                                string[] sequenciaCap = options.Split('-');
                                try
                                {
                                    int cap1 = int.Parse(sequenciaCap[0]) - 1; // -1 para ficar em Array
                                    int cap2 = int.Parse(sequenciaCap[1]) - 1;

                                    if(cap1 >= cap2)
                                    {
                                        for(int i = cap1; i >= cap2; i--)
                                        {
                                            await ajustarMandar(doc, name[i].InnerText, mangaName, i);
                                            
                                            if (i >= cap2)
                                            {
                                                fullDownload = true;
                                                tryAgain = false;
                                            }//para finalizar
                                        }
                                    }
                                    else if(cap2 >= cap1)//5 10
                                    {
                                        for (int i = cap2; i >= cap1; i--)
                                        {
                                            await ajustarMandar(doc, name[i].InnerText, mangaName, i);
                                            
                                            if (i >= cap1)
                                            {
                                                fullDownload = true;
                                                tryAgain = false;
                                            }//para finalizar
                                        }
                                    }
                                }catch
                                {
                                    Console.WriteLine("Escreva por exemplo nessa sintaxe dentro do padrão de escolhas: 20-40");
                                }
                            }
                            if (options.Contains(','))
                            {
                                options = options.Trim();
                                string[] chapters = options.Split(',');

                                try
                                {
                                    for (int i = 0; i <= chapters.Length - 1; i++) // 10/11
                                    {
                                        string chap = chapters[i].Trim();
                                        if(chap != "")
                                        {
                                            int chapNumber = int.Parse(chap) - 1;
                                            try //Um metado para ele identficar se há aqueli capitulo no array
                                            {
                                                await ajustarMandar(doc, name[chapNumber].InnerText, mangaName, chapNumber);
                                            }
                                            catch
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("\nNão foi possível encontrar o capítulo: " + (chapNumber + 1));
                                                Console.ResetColor();
                                            }
                                        }

                                        if (i >= chapters.Length - 1)
                                        {
                                            fullDownload = true;
                                            tryAgain = false;
                                        }//para finalizar
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Escreva por exemplo nessa sintaxe dentro do padrão de escolhas: 10,20,30,40");
                                }
                            }

                            if (executado)
                            {
                                try
                                {
                                    options = options.Trim();
                                    int chapNumber = int.Parse(options) - 1;
                                    await ajustarMandar(doc, name[chapNumber].InnerText, mangaName, chapNumber);
                                    fullDownload = true;
                                    tryAgain = false;
                                }
                                catch
                                {
                                    Console.WriteLine("Capitulo(s) não encontrados. De novo: Quais capítulos você deseja instalar? ");
                                }
                            }
                            break;
                    }
                }
            }
        }

        static async Task ajustarMandar(HtmlAgilityPack.HtmlDocument doc, string name, string mangaName, int i)
        {
            //Os tratamentos de erro acontece antes da chamada da função. Se acontecer ela notificará o erro e terá que ser reaberta para recomeçar
            try
            {

                var chapersLinkBruto = doc.DocumentNode.SelectNodes("//div[@class='pop-content']/div[@class='tags']/a");
                string stringLink = chapersLinkBruto[i].OuterHtml;
                string link = stringLink.Replace("<a href='", "");
                link = link.Substring(0, link.IndexOf("'"));
                string nameCap = name;
                await ImgChapter(link, mangaName, nameCap);
                await Task.Delay(1000); //para não haver erro por estar muito rapido na requisicão
            }catch
            {
                Console.WriteLine("Erro instalar capítulo/img");
            }
        }

        static async Task ImgChapter(string chapterURL, string mangaName, string cap)
        {
            var wc = new HtmlWeb();
            var doc = wc.Load(chapterURL);

            var imgURL = doc.DocumentNode.SelectNodes("//div[@class='read-slideshow']/a/picture/img");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nInstalando capítulo: " + cap);
            Console.ResetColor();
            if(imgURL != null)
            {
                for( int i = 0; i <= imgURL.Count - 1; i++)
                {
                    string replaceVariable = "<img id='img_" + (i + 1) + "' src='"; //possivel erro no futuro caso haja atualizações no site
                    string linkImg = imgURL[i].OuterHtml;
                    linkImg = linkImg.Replace(replaceVariable, "");
                    linkImg = linkImg.Substring(0, linkImg.IndexOf("'"));

                    string caminho = folder + @"\" + mangaName + @"\" + cap + @"\";
                    string pageNumber = Convert.ToString(i + 1);

                    await DirectoryControl(folder); //Cria a pasta principal
                    await DirectoryControl(folder + @"\" + mangaName); //cria a pasta do nome do anime
                    await DirectoryControl(folder + @"\" + mangaName + @"\" + cap); //cria a pasta capitulos
                    await SalvarIMG(linkImg, caminho, pageNumber);
                }
            }
            else
            {
                Console.WriteLine("Erro ao instalar o cap: " + (cap + 1) + " fora do formato array. Para isso subtraia 1. Informe esse problema ao desenvolvedor!\nValor da variável imgURL: " + imgURL);
            }
        }

        static async Task DirectoryControl(string chapterDir)
        {
            try
            {
                if (!Directory.Exists(chapterDir))
                {
                    Directory.CreateDirectory(chapterDir);
                }
            }catch
            {
                Console.WriteLine("Não foi possivel criar a pasta.");
            }
        }

        static string CaracteresNaoPermitidos(string remover)
        {
            try
            {
                string result = remover.Replace(@"\", "");
                result = result.Replace("/", "");
                result = result.Replace(":", "");
                result = result.Replace("*", "");
                result = result.Replace("?", "");
                result = result.Replace('"', ' ');
                result = result.Replace("<", "");
                result = result.Replace(">", "");
                result = result.Replace("|", "");

                return result;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Não foi possivel formatar.");
                Console.ResetColor();
                return "defaultName - Erro ao corrigir caracteres";
            }
        }

        static async Task SalvarIMG(string imgURL, string dir, string pageNumber)
        {
            var response = await client.GetAsync(imgURL);
            
            if(!File.Exists(dir + pageNumber + ".png")) //Se a imagem ja existir ele não vai instalar
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n" + imgURL);
                Console.ResetColor();
                Console.WriteLine("Baixando: |--=---=---=---=---=---=---=---=---=---=---=---=---=---=---|");

                using (var fs = new FileStream(dir + pageNumber + ".png", FileMode.CreateNew))
                {
                    var result = response.Content.CopyToAsync(fs);
                    result.Wait();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\nImagem ja existente: " + dir + pageNumber + ".png");
                Console.ResetColor();
            }
        }
    }
}
