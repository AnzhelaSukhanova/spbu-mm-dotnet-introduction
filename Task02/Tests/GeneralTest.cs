using System;
using NUnit.Framework;
using Nullability;


namespace Tests
{
    public class Tests
    {
        [Test]
        public void AllFieldsAreValid_1()
        {
            var car = new Car(new Person(new Identifier("James", "Bond")));
            var result = NullabilityUtils.NullableGet<Car, string>(car, "Owner.Id.FirstName");
            Assert.AreEqual("James", result);
        }

        [Test]
        public void AllFieldsAreValid_2()
        {
            var id = new Identifier("James", "Bond");
            var car = new Car(new Person(id));
            var result = NullabilityUtils.NullableGet<Car, Identifier>(car, "Owner.Id");
            Assert.AreEqual(id, result);
        }
        
        [Test]
        public void PrivateFieldAccess()
        {
            var car = new Car(new Person(new Identifier("James", "Bond")));
            var ex = Assert.Throws<ArgumentException>(delegate
            {
                NullabilityUtils.NullableGet<Car, string>(car, "Owner.Id.LastName");
            });
            Assert.That(ex?.Message, Is.EqualTo("Field LastName was not found in Identifier, or can not be accessed."));
        }

        [Test]
        public void LastFieldIsNull()
        {
            var car = new Car(new Person(new Identifier(null, "Bond")));
            var result = NullabilityUtils.NullableGet<Car, string>(car, "Owner.Id.FirstName");
            Assert.IsNull(result);
        }

        [Test]
        public void NextToLastFieldIsNull()
        {
            var car = new Car(new Person(null));
            var result = NullabilityUtils.NullableGet<Car, string>(car, "Owner.Id.FirstName");
            Assert.IsNull(result);
        }

        [Test]
        public void InitialObjectIsNull()
        {
            Car car = null;
            var result = NullabilityUtils.NullableGet<Car, string>(car, "Owner.Id.FirstName");
            Assert.IsNull(result);
        }

        [Test]
        public void EmptyFieldSequenceThrowsException()
        {
            var car = new Car(new Person(new Identifier("James", "Bond")));
            Assert.Throws<ArgumentException>(delegate { NullabilityUtils.NullableGet<Car, string>(car, ""); });
        }
        
        [Test]
        public void InvalidFieldSequenceThrowsException()
        {
            var car = new Car(new Person(new Identifier("James", "Bond")));
            Assert.Throws<ArgumentException>(delegate { NullabilityUtils.NullableGet<Car, string>(car, "Owner.X"); });
        }
    }
}