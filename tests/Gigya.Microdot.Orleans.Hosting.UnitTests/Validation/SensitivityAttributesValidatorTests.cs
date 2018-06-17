﻿using System;
using System.Threading.Tasks;
using Gigya.Common.Contracts.Exceptions;
using Gigya.Common.Contracts.HttpService;
using Gigya.Microdot.Fakes;
using Gigya.Microdot.Hosting.HttpService;
using Gigya.Microdot.Hosting.Validators;
using Gigya.Microdot.Testing.Shared;
using Gigya.ServiceContract.Attributes;
using Ninject;
using NSubstitute;
using NUnit.Framework;

namespace Gigya.Common.Application.UnitTests.Validation
{
    [TestFixture]

    public class SensitivityAttributesValidatorTests
    {
        private const int Port = 0;

        private IValidator _serviceValidator;
        private IServiceInterfaceMapper _serviceInterfaceMapper;
        private Type[] _typesToValidate;

        [SetUp]
        public void Setup()
        {
            _serviceInterfaceMapper = Substitute.For<IServiceInterfaceMapper>();
            _serviceInterfaceMapper.ServiceInterfaceTypes.Returns(_ => _typesToValidate);
            var unitTesting = new TestingKernel<ConsoleLog>(kernel => kernel.Rebind<IServiceInterfaceMapper>().ToConstant(_serviceInterfaceMapper));
            _serviceValidator = unitTesting.Get<SensitivityAttributesValidator>();
        }

        [TestCase(typeof(TwoAttributeOnTheSameMethod))]
        [TestCase(typeof(TwoAttributeOnTheeSameParameter))]
        [TestCase(typeof(IInvalid_WithoutLogFieldAndWithSensitivityOnProperty))]
        [TestCase(typeof(IInvalid_WithoutLogFieldAndWithSensitivityOnField))]

        public void ValidationShouldFail(Type typeToValidate)
        {
            _typesToValidate = new[] { typeToValidate };
            Assert.Throws<ProgrammaticException>(_serviceValidator.Validate);
        }

        [TestCase(typeof(IValidMock))]
        [TestCase(typeof(IComplexParameterValidation))]

        public void ValidationShouldSucceed(Type typeToValidate)
        {
            _typesToValidate = new[] { typeToValidate };
            _serviceValidator.Validate();
        }


        [HttpService(Port, Name = "This service contains methods with valid parameter types")]
        private interface TwoAttributeOnTheSameMethod
        {
            [Sensitive]
            [NonSensitive]
            Task NotValid();
        }

        [HttpService(Port, Name = "This service contains methods with valid parameter types")]
        private interface TwoAttributeOnTheeSameParameter
        {
            Task NotValid([Sensitive] [NonSensitive]string test);
        }


        #region IInvalid_WithoutLogFieldAndWithSensitivityOnProperty

        [HttpService(Port, Name = "This service contains methods with invalid parameter types")]
        private interface IInvalid_WithoutLogFieldAndWithSensitivityOnProperty
        {
            Task NotValid(SmallSchoolWithhAttribute smallSchoolWithhAttribute);
        }


        public class SmallSchoolWithhAttribute
        {
            [Sensitive]
            public string Address { get; set; } = "Bad";
            public string FieldAddress = "Bad";
        }

        #endregion

        #region IInvalid_WithoutLogFieldAndWithSensitivityOnField

        [HttpService(Port, Name = "This service contains methods with invalid parameter types")]
        private interface IInvalid_WithoutLogFieldAndWithSensitivityOnField
        {
            Task NotValid(SmallSchoolWithhAttributeOnField test);
        }


        public class SmallSchoolWithhAttributeOnField
        {
            public class StudentWithFieldAttribute
            {
                [Sensitive]
                public string StudentName = "Maria";

                public string FamilyName { get; set; } = "Bad";

                public int Age { get; set; } = 20;
            }

            public string FieldName = "Maria";
            public string SchoolName = "Maria";

            public string Address { get; set; } = "Bad";
            public string FieldAddress { get; set; } = "Bad";

            public StudentWithFieldAttribute Student { get; set; } = new StudentWithFieldAttribute();


        }

        #endregion

        #region IComplexParameterValidation
        [HttpService(Port, Name = "This service contains valid methods.")]
        public interface IComplexParameterValidation
        {

            Task CreateSchoolWithLogField(SmallSchool school);
            Task CreateSchoolWithLogField(SchoolValidatorWithoutAttributes schoolValidator1);
            Task CreateSchoolWithLogField(SchoolValidatorWithoutAttributes schoolValidator1, SchoolValidatorWithoutAttributes schoolValidator2);
            Task CreateSchoolWithLogField(SchoolValidatorWithoutAttributes schoolValidator1, SchoolValidatorWithoutAttributes schoolValidator2, string test);


            Task CreateSchoolWithoutLogField([LogFields]SchoolValidatorWithoutAttributes schoolValidator1);
            Task CreateSchoolWithoutLogField([LogFields]SchoolValidatorWithoutAttributes schoolValidator1, SchoolValidatorWithoutAttributes schoolValidator2);
            Task CreateSchoolWithoutLogField([LogFields]SchoolValidatorWithoutAttributes schoolValidator1, SchoolValidatorWithoutAttributes schoolValidator2, string test);

        }

        public class SchoolValidatorWithoutAttributes
        {
            public class StudentWithoutAttribute
            {
                public string Name { get; set; } = "Maria";

                public string FamilyName { get; set; } = "Bad";

                public int Age { get; set; } = 20;
            }


            public string FieldName = "Maria";
            public string SchoolName = "Maria";

            public string Address { get; set; } = "Bad";
            public string FieldAddress { get; set; } = "Bad";

            public StudentWithoutAttribute Student { get; set; } = new StudentWithoutAttribute();
        }

        public class SmallSchool
        {

            public string FieldName { get; set; } = "Maria";
            public string SchoolName { get; set; } = "Maria";
        }
        #endregion


        private interface IValidMock
        {
            Task valid([Sensitive] string test);
            Task valid2([NonSensitive] string test);
            [NonSensitive]
            Task valid3([NonSensitive] string test);
            [Sensitive]
            Task valid4([NonSensitive] string test);

            [NonSensitive]
            Task valid5([Sensitive] string test);
            [Sensitive]
            Task valid6([Sensitive] string test);
            Task valid7([Sensitive] string test, [NonSensitive] string test2);



        }
    }
}