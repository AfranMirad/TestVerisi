using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TestVerisiOlustur.Models;

namespace TestVerisiOlustur.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestVerisiController : ControllerBase
    {
        string BaglantiAdresi = "Server=.;Database=TestVerisiOlusturDB;Trusted_Connection=True;";
        private readonly ILogger<TestVerisiController> _logger;

        public TestVerisiController(ILogger<TestVerisiController> logger)
        {
            _logger = logger;
        }

        private static readonly string[] Sehirler = new[]
       {
            "Ankara", "İstanbul", "Bursa", "Edirne", "Konya", "Antalya", "Diyarbakır", "Van", "Rize", "İzmir"
        };

        [Route("TestVerisiOlustur")]
        [HttpPost]
        public async Task<IActionResult> TestVerisiOlustur(int musteriAdet, int sepetAdet)
        {
            SqlConnection Baglanti = new SqlConnection();
            Baglanti.ConnectionString = BaglantiAdresi;
            Baglanti.Open();
            var Rnd = new Random();
            List<Musteri> Musteriler = new List<Musteri>();
            List<Sepet> Sepetler = new List<Sepet>();
            for (int i = 0; i < musteriAdet; i++)
            {
                string Ad = "";
                for (int adsayac = 0; adsayac < 5; adsayac++)
                {
                    Ad += ((char)Rnd.Next('A', 'Z')).ToString();
                }

                string Soyad = "";
                for (int adsayac = 0; adsayac < 5; adsayac++)
                {
                    Soyad += ((char)Rnd.Next('A', 'Z')).ToString();
                }

                int SehirId = Rnd.Next(1, 10);

                Baglanti.Query("INSERT INTO Musteri(Ad,Soyad,Sehir) VALUES ('" + Ad + "','" + Soyad + "','" + Sehirler[SehirId].ToString() + "')");
            }

            Musteriler = Baglanti.Query<Musteri>("select top " + musteriAdet + " * from Musteri order by Id desc").ToList();

            for (int i = 0; i < sepetAdet; i++)
            {
                int MusteriId = Rnd.Next(Musteriler[musteriAdet - 1].Id, Musteriler[0].Id);
                Baglanti.Query("INSERT INTO Sepet(MusteriId) VALUES (" + MusteriId + ")");
            }

            Sepetler = Baglanti.Query<Sepet>("select top " + sepetAdet + " * from Sepet order by Id desc").ToList();

            foreach (var sepet in Sepetler)
            {
                int SepetDonus = Rnd.Next(1, 5);
                for (int i = 0; i < SepetDonus; i++)
                {
                    int Tutar = Rnd.Next(100, 1000);

                    string Aciklama = "";
                    for (int Aciklamasayac = 0; Aciklamasayac < 15; Aciklamasayac++)
                    {
                        Aciklama += ((char)Rnd.Next('A', 'Z')).ToString();
                    }

                    string SorguSepetUrunEkle = "INSERT INTO UrunSepet(SepetId,Tutar,Aciklama) VALUES (" + sepet.Id + "," + Tutar + ",'" + Aciklama + "')";
                    Baglanti.Query(SorguSepetUrunEkle);
                }
            }

            return Ok("İşlem Başarılı");
        }

        [Route("SehirBazliAnalizYap")]
        [HttpGet]
        public async Task<IActionResult> SehirBazliAnalizYap()
        {
            IEnumerable<DtoSehirAnaliz> DtoSehirAnalizList = new List<DtoSehirAnaliz>();

            SqlConnection Baglanti = new SqlConnection();
            Baglanti.ConnectionString = BaglantiAdresi;
            Baglanti.Open();
            DtoSehirAnalizList = Baglanti.Query<DtoSehirAnaliz>("SELECT M.Sehir As Sehir, Sum(US.Tutar) AS Tutar FROM UrunSepet US INNER JOIN Sepet S ON US.SepetId = S.Id INNER JOIN Musteri M ON S.MusteriId = M.Id GROUP BY M.Sehir");

            AdetModel AdetModel = new AdetModel();
            foreach (var item in DtoSehirAnalizList)
            {
                AdetModel = Baglanti.Query<AdetModel>("select Count(m.Sehir) As Adet from Sepet s inner join Musteri m on s.MusteriId = m.Id where m.Sehir = '" + item.Sehir + "'").FirstOrDefault();
                item.Adet = AdetModel.Adet;
            }

            return Ok(DtoSehirAnalizList);
        }
    }
}
