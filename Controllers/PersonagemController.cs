﻿using System;
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
    public class PersonagemController : Controller
    {
        /// <summary>
        /// Lista todos os personagens Marvel utilizando páginação dinamica
        /// </summary>
        /// <param name="limit">Número máximo de resultados a serem exibidos</param>
        /// <param name="offset">Número de resultados anteriores a serem ignorados</param>
        /// <returns>Lista do tipo personagem e Viewbag com todos os parâmetros para contrução do paginador</returns>
        public ActionResult Index(int limit, int offset)
        {
            try
            {
                Personagem personagem;
                List<Personagem> lista_personagens = new List<Personagem>();

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
                    HttpResponseMessage response = client.GetAsync("https://gateway.marvel.com:443/v1/public/characters?ts=" + ts + "&limit=" + limit + "&offset=" + offset + "&apikey=" + publicKey + "&hash=" + hash).Result;

                    string conteudo = response.Content.ReadAsStringAsync().Result;

                    //Interpreta o JSON retornado pela API
                    dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                    //Define parâmetros de controle para construção da lista sendo size o total retornado, sendo size a quantidade necessaria para o loop e total o total de registros no BD da API
                    size = Convert.ToInt32(resultado.data.count);
                    int total = Convert.ToInt32(resultado.data.total);

                    //Cria a lista de personagens manipulando o JSON
                    for (int j = 0; j < size; j++)
                    {
                        personagem = new Personagem();
                        personagem.Id_marvel = resultado.data.results[j].id;
                        personagem.Nome = resultado.data.results[j].name;
                        personagem.Descricao = resultado.data.results[j].description;
                        personagem.Pic_url = resultado.data.results[j].thumbnail.path + "." +
                            resultado.data.results[j].thumbnail.extension;
                        if (resultado.data.results[j].urls.Count > 0)
                            personagem.Wiki_url = resultado.data.results[j].urls[0].url;
                        lista_personagens.Add(personagem);
                    }

                    //Define parâmetros de paginação
                    int paginator = total / limit;
                    ViewBag.paginator = paginator;
                    ViewBag.limit = limit;
                    ViewBag.offset = offset;
                    ViewBag.total = total;

                }
                return View(lista_personagens);
            }
            catch (Exception e)
            {
                ViewBag.title = "Um erro ocorreu";
                ViewBag.message = "Um erro ocorreu, por favor, tente novamente";
                return View("Message");
            }

        }

        /// <summary>
        /// Salva um único personagem no BD local
        /// </summary>
        /// <param name="id_personagem_marvel">id do personagem no BD da API</param>
        /// <returns>Uma mensagem de status da solicitação</returns>
        public ActionResult SalvarPersonagem(int id_personagem_marvel)
        {
            Personagem personagem;
            try
            {
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
                    HttpResponseMessage response = client.GetAsync("https://gateway.marvel.com:443/v1/public/characters/" + id_personagem_marvel + "?apikey=" + publicKey + "&hash=" + hash + "&ts=" + ts).Result;

                    string conteudo = response.Content.ReadAsStringAsync().Result;

                    //Interpreta o JSON retornado pela API
                    dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                    PersonagemRepositorio pRep = new PersonagemRepositorio();

                    //Verifica se o personagem já existe no BD local
                    personagem = new Personagem();
                    personagem = pRep.FindByIdMarvel(id_personagem_marvel);

                    //Caso não exista cria o obj personagem e o salva localmente
                    if (personagem == null)
                    {
                        Personagem personagem_salvar = new Personagem();
                        personagem_salvar.Id_marvel = resultado.data.results[0].id;
                        personagem_salvar.Nome = resultado.data.results[0].name;
                        personagem_salvar.Descricao = resultado.data.results[0].description;
                        personagem_salvar.Pic_url = resultado.data.results[0].thumbnail.path + "." +
                            resultado.data.results[0].thumbnail.extension;
                        personagem_salvar.Wiki_url = resultado.data.results[0].urls[1].url;
                        pRep.Save(personagem_salvar);

                        ViewBag.title = "Sucesso";
                        ViewBag.message = "Personagem salvo com sucesso";


                        return View("Message");
                    }
                    //Caso já exista retorna mensagem informativa
                    else
                    {
                        ViewBag.title = "Personagem já salvo";
                        ViewBag.message = "Este personagem já existe na base de dados";

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
        /// Exluir um personagem do BD local
        /// </summary>
        /// <param name="id_personagem_marvel">Id do personagem no BD da API</param>
        /// <returns></returns>
        public ActionResult ExcluirPersonagem(int id_personagem_marvel)
        {
            try
            {
                Personagem personagem;
                PersonagemRepositorio pRep = new PersonagemRepositorio();
                //Verifica a existencia do personagem no BD local
                personagem = pRep.FindByIdMarvel(id_personagem_marvel);

                //Caso não exista retorna uma mensagem informativa
                if (personagem == null)
                {
                    ViewBag.title = "Erro";
                    ViewBag.message = "Personagem não encontrado";
                    return View("Message");
                }
                //Caso exista exclui o personagem retorna uma mensagem informativa
                else
                {
                    pRep.RemoveById(personagem.Id);
                    ViewBag.title = "Sucesso";
                    ViewBag.message = "Personagem excluido com sucesso";
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
        /// Lista todos os personagens salvos no BD
        /// </summary>
        /// <returns>Lista do tipo personagem</returns>
        public ActionResult Salvos()
        {
            try
            {
                PersonagemRepositorio pRep = new PersonagemRepositorio();
                List<Personagem> lista_personagem= new List<Personagem>();

                lista_personagem = pRep.ReturnAll();

                ViewBag.total_lista = lista_personagem.Count;
                return View(lista_personagem);
            }
            catch (Exception e)
            {
                ViewBag.title = "Um erro ocorreu";
                ViewBag.message = "Um erro ocorreu, por favor, tente novamente";
                return View("Message");
            }

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
