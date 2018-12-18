﻿namespace Be.Vlaanderen.Basisregisters.Redis.Populator.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Infrastructure;
    using Givens;
    using Fixtures;
    using Model;

    // ReSharper disable InconsistentNaming
    public class WhenGettingTheUnpopulatedRecords_GivenThreeUnpopulated : GivenThreeUnpopulatedRecordsInDb,
        IClassFixture<RedisPopulatorFixture>
    {
        private readonly IRepository _sut;

        public WhenGettingTheUnpopulatedRecords_GivenThreeUnpopulated(RedisPopulatorFixture fixture)
         : base (fixture.CreateInMemoryContext())
        {
            _sut = new Repository(Context);
        }

        [Fact]
        public async Task ThenAllRecordsAreReturned()
        {
            var dbRecords = await _sut.GetUnpopulatedRecordsAsync(1000);

            Assert.NotEmpty(dbRecords);
            Assert.Equal(Records.Count, dbRecords.Count);
            Assert.True(dbRecords.TrueForAll(r => r.Position > r.LastPopulatedPosition));
        }
    }

    public class WhenGettingTheUnpopulatedRecords_GivenTwoUnpopulated : GivenTwoUnpopulatedRecordsAndOnePopulatedRecordInDb,
        IClassFixture<RedisPopulatorFixture>
    {
        private readonly IRepository _sut;
        private LastChangedRecord _populatedRecord;

        public WhenGettingTheUnpopulatedRecords_GivenTwoUnpopulated(RedisPopulatorFixture fixture)
            : base (fixture.CreateInMemoryContext())
        {
            _sut = new Repository(Context);
        }

        [Fact]
        public async Task ThenAllUnpopulatedRecordsAreReturned()
        {
            var allDbRecords = Context.LastChangedList.ToList();
            Assert.Equal(Records.Count, allDbRecords.Count);

            _populatedRecord = allDbRecords.FirstOrDefault(r => r.Position == r.LastPopulatedPosition);
            Assert.NotNull(_populatedRecord);

            var unpopulatedRecords = await _sut.GetUnpopulatedRecordsAsync(1000);

            Assert.NotEmpty(unpopulatedRecords);
            Assert.Equal(2, unpopulatedRecords.Count);
            Assert.True(unpopulatedRecords.TrueForAll(r => r.Position > r.LastPopulatedPosition));
            Assert.DoesNotContain(unpopulatedRecords, r => r.Id == _populatedRecord.Id);
        }
    }

    public class WhenGttingTheUnpopulatedRecords_GivenOneError : GivenOneErrorRecordInDb,
        IClassFixture<RedisPopulatorFixture>
    {
        private readonly IRepository _sut;

        public WhenGttingTheUnpopulatedRecords_GivenOneError(RedisPopulatorFixture fixture)
            : base(fixture.CreateInMemoryContext())
        {
            _sut = new Repository(Context);
        }

        [Fact]
        public async Task ThenOnlyTheRecordsWithoutErrorsAreReturned()
        {
            var unpopulatedRecords = await _sut.GetUnpopulatedRecordsAsync(1000);

            Assert.NotEmpty(unpopulatedRecords);
            Assert.Equal(2, unpopulatedRecords.Count);
            Assert.True(unpopulatedRecords.TrueForAll(r => r.Position > r.LastPopulatedPosition));
            Assert.True(unpopulatedRecords.TrueForAll(r => r.HasErrors == false));
        }
    }
}
