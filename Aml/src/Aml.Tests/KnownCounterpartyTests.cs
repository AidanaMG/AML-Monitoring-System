using Aml.Domain.Entities;
using Aml.Domain.Enums;

namespace Aml.Tests
{
    public class KnownCounterpartyTests
    {
        [Fact]
        public void Create_ValidName_ReturnsValidNormalizedNameRemovedDateIsNull()
        {
            var name = "  Ivan Petrovich  ";
            var type = CounterpartyType.AntifraudListed;
            var source = "Antifraud";
            var addedDate = DateTime.UtcNow;

            var result = KnownCounterparty.Create(name, type, source, addedDate);

            Assert.Equal("Ivan Petrovich", result.Name);
            Assert.Equal("IVAN PETROVICH", result.NormalizedName);
            Assert.Equal(type, result.Type);
            Assert.Equal(source, result.Source);
            Assert.Equal(addedDate, result.AddedDate);
            Assert.Null(result.RemovedDate);

        }


        [Fact]
        public void Create_EmptyName_ThrowArgumentException()
        {
            var name = "    ";
            var exception = Assert.Throws<ArgumentException>(() =>
                KnownCounterparty.Create(name, CounterpartyType.Gambling, null, DateTime.UtcNow));

            Assert.Equal("name", exception.ParamName);
        }


        [Fact]
        public void Create_WrongFormatName_ReturnsValidNormalizedNameRemovedDateIsNul()
        {
            var name = "  maXIm    peTROvich  ";
            var type = CounterpartyType.AntifraudListed;
            var source = "Antifraud";
            var addedDate = DateTime.UtcNow;

            var result = KnownCounterparty.Create(name, type, source, addedDate);

            Assert.Equal("maXIm peTROvich", result.Name);
            Assert.Equal("MAXIM PETROVICH", result.NormalizedName);
            Assert.Equal(type, result.Type);
            Assert.Equal(source, result.Source);
            Assert.Equal(addedDate, result.AddedDate);
            Assert.Null(result.RemovedDate);
        }

        [Fact]
        public void Create_NullName_ThrowArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                KnownCounterparty.Create(null, CounterpartyType.Gambling, null, DateTime.UtcNow));

            Assert.Equal("name", exception.ParamName);
        }




        [Fact]
        public void Deactivate_SetRemovedDate_ReturnsSetRemovedDate()
        {
            DateTime removedDate = DateTime.UtcNow.AddMinutes(30);
            var counterparty = KnownCounterparty.Create("Maxim Petrovich", CounterpartyType.Drug, "Police Department", DateTime.UtcNow);

            counterparty.Deactivate(removedDate);

            Assert.Equal(removedDate, counterparty.RemovedDate);
        }


        [Fact]
        public void Deactivate_SetIncorrectRemovedDate_ThrowInvalidOperationException()
        {
            var counterparty = KnownCounterparty.Create("Irina Petrovna", CounterpartyType.Drug, "Police Department", DateTime.UtcNow);

            var exception = Assert.Throws<InvalidOperationException>(() =>
               counterparty.Deactivate(DateTime.UtcNow.AddDays(-5)));

            Assert.StartsWith("Unable to deactivate: removal date is earlier than addition date", exception.Message);
        }

        [Fact]
        public void Deactivate_SetRemovedDateEqualAddedDate_ReturnsSetRemovedDate()
        {
            var addedDate = DateTime.UtcNow;
            var removedDate = addedDate;
            var counterparty = KnownCounterparty.Create("Maya Petrovna", CounterpartyType.Drug, "Police Department", addedDate);

            counterparty.Deactivate(removedDate);

            Assert.Equal(removedDate, counterparty.RemovedDate);

        }

        [Fact]
        public void Deactivate_RemovedDateIsSet_ThrowInvalidOperationException()
        {

            var counterparty = KnownCounterparty.Create("Irina Petrovna", CounterpartyType.Drug, "Police Department", DateTime.UtcNow);
            counterparty.Deactivate(DateTime.UtcNow);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                counterparty.Deactivate(DateTime.UtcNow));

            Assert.StartsWith("Unable to deactivate: removal date is already set", exception.Message);


        }
    }
}
