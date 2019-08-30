using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Marvel.Models;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Marvel.Controllers
{
    public class ComicController : Controller
    {
        /// <summary>
        /// Lista todos os personagens Marvel utilizando páginação dinamica
        /// </summary>
        /// <param name="id_personagem">Id da comic no BD da API</param>
        /// <param name="limit">Número máximo de resultados a serem exibidos</param>
        /// <param name="offset">Número de resultados anteriores a serem ignorados</param>
        /// <returns>Lista do tipo comic e Viewbag com todos os parâmetros para contrução do paginador</returns>
        public ActionResult Index(int id_personagem, int limit, int offset)
        {
            try
            {
                Comic comic;
                List<Comic> lista_comic = new List<Comic>();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    //Define chaves e váriaveis de conexão para API

                    string ts = DateTime.Now.Ticks.ToString();
                    string publicKey = "3dac5671135fab935c77add36e46abb6";
                    string hash = GerarHash(ts, publicKey, "c109c5ae5fe25f81bc149b4cf7a2407678d5dc40");

                    int size;

                    //Realiza a requisição da API
                    HttpResponseMessage response = client.GetAsync("https://gateway.marvel.com:443/v1/public/characters/" + id_personagem + "/comics?ts=" + ts + "&limit=" + limit + "&offset=" + offset + "&apikey=" + publicKey + "&hash=" + hash).Result;

                    string conteudo = response.Content.ReadAsStringAsync().Result;

                    //Interpreta o JSON retornado pela API
                    dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                    //Define parâmetros de controle para construção da lista sendo size o total retornado, sendo size a quantidade necessaria para o loop e total o total de registros no BD da API
                    size = Convert.ToInt32(resultado.data.count);
                    int total = Convert.ToInt32(resultado.data.total);

                    //Cria a lista de personagens manipulando o JSON
                    for (int j = 0; j < size; j++)
                    {
                        comic = new Comic();
                        comic.Id_marvel = resultado.data.results[j].id;
                        comic.Titulo = resultado.data.results[j].title;
                        comic.Descricao = resultado.data.results[j].description;
                        comic.Preco = resultado.data.results[j].prices[0].price;
                        comic.Pic_url = resultado.data.results[j].thumbnail.path + "." +
                            resultado.data.results[j].thumbnail.extension;
                        if (resultado.data.results[j].urls.Count > 0)
                            comic.Wiki_url = resultado.data.results[j].urls[0].url;
                        lista_comic.Add(comic);
                    }

                    //Define parâmetros de paginação
                    int paginator = total / limit;
                    ViewBag.paginator = paginator;
                    ViewBag.limit = limit;
                    ViewBag.offset = offset;
                    ViewBag.id_personagem = id_personagem;
                    ViewBag.total = total;
                    ViewBag.total_lista = lista_comic.Count;

                }
                return View(lista_comic);
            }
            catch (Exception e)
            {
                ViewBag.title = "Um erro ocorreu";
                ViewBag.message = "Um erro ocorreu, por favor, tente novamente";
                return View("Message");
            }

        }

        /// <summary>
        /// Salva uma única comic no BD local
        /// </summary>
        /// <param name="id_comic_marvel">id da comic no BD da API</param>
        /// <returns>Uma mensagem de status da solicitação</returns>
        public ActionResult SalvarComic(int id_comic_marvel)
        {
            try
            {
                Comic comic;
                List<Comic> lista_comic = new List<Comic>();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    //Define chaves e váriaveis de conexão para API
                    string ts = DateTime.Now.Ticks.ToString();
                    string publicKey = "3dac5671135fab935c77add36e46abb6";
                    string hash = GerarHash(ts, publicKey, "c109c5ae5fe25f81bc149b4cf7a2407678d5dc40");

                    //Realiza a requisição da API
                    HttpResponseMessage response = client.GetAsync("https://gateway.marvel.com:443/v1/public/comics/" + id_comic_marvel + "?ts=" + ts + "&apikey=" + publicKey + "&hash=" + hash).Result;

                    string conteudo = response.Content.ReadAsStringAsync().Result;

                    //Interpreta o JSON retornado pela API
                    dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                    ComicRepositorio cRep = new ComicRepositorio();

                    //Verifica se o personagem já existe no BD local
                    comic = new Comic();
                    comic = cRep.FindByIdMarvel(id_comic_marvel);

                    //Caso não exista cria o obj personagem e o salva localmente
                    if (comic == null)
                    {
                        Comic comic_salvar = new Comic();
                        comic_salvar.Id_marvel = resultado.data.results[0].id;
                        comic_salvar.Titulo = resultado.data.results[0].title;
                        comic_salvar.Descricao = resultado.data.results[0].description;
                        comic_salvar.Pic_url = resultado.data.results[0].thumbnail.path + "." +
                            resultado.data.results[0].thumbnail.extension;
                        comic_salvar.Wiki_url = resultado.data.results[0].urls[0].url;
                        cRep.Save(comic_salvar);

                        ViewBag.title = "Sucesso";
                        ViewBag.message = "Comic salva com sucesso";

                        return View("Message");
                    }
                    //Caso já exista retorna mensagem informativa
                    else
                    {
                        ViewBag.title = "Comic já salva";
                        ViewBag.message = "Esta comic já existe na base de dados";

                        return View("Message");
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.title = "Um erro ocorreu";
                ViewBag.message = "Um erro ocorreu, por favor, tente novamente";
                return View("Message");
            }

        }

        /// <summary>
        /// Exluir uma única comic do BD local
        /// </summary>
        /// <param name="id_comic_marvel">Id da comic no BD da API</param>
        /// <returns></returns>
        public ActionResult ExcluirComic(int id_comic_marvel)
        {            
            try
            {
                Comic comic;
                ComicRepositorio cRep = new ComicRepositorio();
                //Verifica a existencia da comic no BD local
                comic = cRep.FindByIdMarvel(id_comic_marvel);

                //Caso não exista retorna uma mensagem informativa
                if (comic == null)
                {
                    ViewBag.title = "Erro";
                    ViewBag.message = "Comic não encontrada";
                    return View("Message");
                }
                //Caso exista exclui o personagem retorna uma mensagem informativa
                else
                {
                    cRep.RemoveById(comic.Id);
                    ViewBag.title = "Sucesso";
                    ViewBag.message = "Comic excluida com sucesso";
                    return View("Message");
                }
            }
            catch (Exception e)
            {
                ViewBag.title = "Um erro ocorreu";
                ViewBag.message = "Um erro ocorreu, por favor, tente novamente";
                return View("Message");
            }

        }

        /// <summary>
        /// Salva todos os personagens do BD da API no BD local
        /// </summary>
        /// <returns></returns>
        public ActionResult SalvarComics()
        {
            Comic comic;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                string ts = DateTime.Now.Ticks.ToString();
                string publicKey = "3dac5671135fab935c77add36e46abb6";
                string hash = GerarHash(ts, publicKey, "c109c5ae5fe25f81bc149b4cf7a2407678d5dc40");

                List<Comic> lista_comic = new List<Comic>();

                int limit = 20;
                int offset = 0;
                int count = 0;
                int size = limit;
                int total = 51;

                do
                {
                    HttpResponseMessage response = client.GetAsync("https://gateway.marvel.com:443/v1/public/comics?ts=" + ts + "&limit=" + limit + "&offset=" + offset + "&apikey=" + publicKey + "&hash=" + hash).Result;
                    offset += 20;

                    string conteudo = response.Content.ReadAsStringAsync().Result;

                    dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                    size = Convert.ToInt32(resultado.data.count);
                    count += size;
                    total = Convert.ToInt32(resultado.data.total);

                    for (int j = 0; j < size; j++)
                    {
                        comic = new Comic();
                        comic.Id = resultado.data.results[j].id;
                        comic.Titulo = resultado.data.results[j].name;
                        comic.Descricao = resultado.data.results[j].description;
                        comic.Pic_url = resultado.data.results[j].thumbnail.path + "." +
                            resultado.data.results[j].thumbnail.extension;
                        comic.Wiki_url = resultado.data.results[j].urls[1].url;
                        lista_comic.Add(comic);
                    }
                }
                while (count < total);

                lista_comic = null;

            }

            return View();
        }

        /// <summary>
        /// Gera um hash MD5 para a API
        /// </summary>
        /// <param name="ts">Timespan gerado</param>
        /// <param name="publicKey">Chave pública da API</param>
        /// <param name="privateKey">Chave privada da API</param>
        /// <returns>Hash MD5</returns>
        private string GerarHash(
            string ts, string publicKey, string privateKey)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(ts + privateKey + publicKey);
            var gerador = MD5.Create();
            byte[] bytesHash = gerador.ComputeHash(bytes);
            return BitConverter.ToString(bytesHash)
                .ToLower().Replace("-", String.Empty);
        }

    }
}
