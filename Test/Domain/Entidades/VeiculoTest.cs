using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using trilha_net_minimals_api.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculoTest
    {
        [TestMethod]
        public void TestarGetSetPropiedades()
        {
            // Arrange
            var veiculo = new Veiculo();

            //Act
            veiculo.Id = 1;
            veiculo.Nome = "Honda Fit";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2022;

            //Assert
            Assert.AreEqual(1, veiculo.Id);
            Assert.AreEqual("Honda Fit", veiculo.Nome);
            Assert.AreEqual("Honda", veiculo.Marca);
            Assert.AreEqual(2022, veiculo.Ano);
        }
    }
}