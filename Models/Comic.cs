using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Marvel.Models
{
    public class Comic
    {
        //ID local da comic
        public int Id { get; set; }
        //ID no BD API Marvel
        public int Id_marvel { get; set; }
        //Titulo da comic
        public string Titulo { get; set; }
        //Descrição da comic
        public string Descricao { get; set; }
        //Preço de venda da comic
        public float? Preco { get; set; }
        //Link para imagem da comic
        public string Pic_url { get; set; }
        //Link para wiki da comic
        public string Wiki_url { get; set; }
    }
}