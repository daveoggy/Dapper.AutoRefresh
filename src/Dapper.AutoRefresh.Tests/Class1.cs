using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Dapper.AutoRefresh.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public async Task T01()
        {
            // Arrange
            var sqlDependencyFactoryMock = new Mock<ISqlDependencyAdapterFactory>();
            var sqlDependencyMock = new Mock<ISqlDependencyAdapter>();
            var dbConnectionFactory = new Mock<IDbConnectionFactory>();
            var dapperAdapter = new Mock<IDapperAdapter>();

            sqlDependencyFactoryMock.Setup(factory => factory.Create(It.IsAny<string>()))
                .Returns(sqlDependencyMock.Object);

            dapperAdapter.Setup(adapter => adapter.QueryAsync<TestType>(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int>(),
                It.IsAny<CommandType>()));
            
            var sut = new SqlAutoRefresh<TestType>("SELECT 1 FROM Test", "TestConnStr", 
                sqlDependencyAdapterFactory: sqlDependencyFactoryMock.Object,
                dbConnectionFactory: dbConnectionFactory.Object,
                dapperAdapter: dapperAdapter.Object);

            async Task Function()
            {
                await Task.Delay(10000);
                sqlDependencyMock.Raise(adapter => adapter.OnChange += null, new SqlNotificationEventArgs(SqlNotificationType.Change, SqlNotificationInfo.Alter, SqlNotificationSource.Data));
            }

            var delayFiringChangeEvent = Task.Run(Function);

            // Act
            for (int i = 0; i < 2; i++)
            {
                var result = await sut.GetLatest();
            }

            await delayFiringChangeEvent;

            // Assert
            Assert.Pass("Figure stuff out again");
        }
    }
}
