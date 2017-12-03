using Fapl.Flashcards.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fapl.Flashcards.Tests
{
    [TestClass]
    public class SynchronizerTests
    {
        private readonly IPet pet1 = Mock.Of<IPet>(p => p.ID == "p1" && p.LastUpdateTimestamp == DateTime.UtcNow);
        private readonly IPet pet2 = Mock.Of<IPet>(p => p.ID == "p2" && p.LastUpdateTimestamp == DateTime.UtcNow);
        private readonly IPet pet3 = Mock.Of<IPet>(p => p.ID == "p3" && p.LastUpdateTimestamp == DateTime.UtcNow);

        private readonly IFlashcard flashcard1 = Mock.Of<IFlashcard>(f => f.ID == "f1");
        private readonly IFlashcard flashcard2 = Mock.Of<IFlashcard>(f => f.ID == "f2");
        private readonly IFlashcard flashcard3 = Mock.Of<IFlashcard>(f => f.ID == "f3");

        private readonly SyncOptions options = new SyncOptions();

        protected IFlashcardSet MockFlashcardSet(IEnumerable<IFlashcard> flashcards)
        {
            var mock = new Mock<IFlashcardSet>();
            mock.Setup(s => s.GetEnumerator()).Returns(flashcards.GetEnumerator());
            mock.Setup(s => s.CreateFlashcard()).Returns(Mock.Of<IFlashcard>());
            return mock.Object;
        }

        protected ISyncService MockSyncService()
            => Mock.Of<ISyncService>(s => s.Save(It.IsAny<SyncMetadata>()) == Task.FromResult(0));

        protected Synchronizer MockSynchronizer(ISyncService syncService)
        {
            var mock = new Mock<Synchronizer>(null, null, syncService, null);
            mock.CallBase = true;
            mock.Setup(s => s.AddFlashcard(It.IsAny<IFlashcardSet>(), It.IsAny<IPet>(), It.IsAny<SyncMetadata>(), It.IsAny<SyncOptions>()))
                .Returns(Task.FromResult(0));
            mock.Setup(s => s.UpdateFlashcard(It.IsAny<IFlashcardSet>(), It.IsAny<IFlashcard>(), It.IsAny<IPet>(), It.IsAny<SyncMetadata>(), It.IsAny<SyncOptions>()))
                .Returns(Task.FromResult(0));
            mock.Setup(s => s.RemoveFlashcard(It.IsAny<IFlashcardSet>(), It.IsAny<IFlashcard>(), It.IsAny<SyncMetadata>()))
                .Returns(Task.FromResult(0));
            return mock.Object;
        }

        [TestMethod]
        public void AddsFlashcards()
        {
            IFlashcardSet flashcardSet = MockFlashcardSet(new[] { flashcard1 });
            SyncMetadata metadata = new SyncMetadata
            {
                FlashcardIDs = new Dictionary<string, string> { { "p1", "f1" } },
                PetLastUpdateTimestamps = new Dictionary<string, DateTime> { { "p1", DateTime.UtcNow } },
                PetIDs = new Dictionary<string, string> { { "f1", "p1" } }
            };
            ISyncService syncService = MockSyncService();
            Synchronizer synchronizer = MockSynchronizer(syncService);

            synchronizer.SynchronizeFlashcards(flashcardSet, new[] { pet1, pet2, pet3 }, metadata, options).Wait();

            Mock.Get(synchronizer).Verify(s => s.AddFlashcard(flashcardSet, pet1, metadata, options), Times.Never);
            Mock.Get(synchronizer).Verify(s => s.AddFlashcard(flashcardSet, pet2, metadata, options));
            Mock.Get(synchronizer).Verify(s => s.AddFlashcard(flashcardSet, pet3, metadata, options));
            Mock.Get(synchronizer).Verify(s => s.UpdateFlashcard(flashcardSet, It.IsAny<IFlashcard>(), It.IsAny<IPet>(), metadata, options), Times.Never());
            Mock.Get(synchronizer).Verify(s => s.RemoveFlashcard(flashcardSet, It.IsAny<IFlashcard>(), metadata), Times.Never());
            Mock.Get(syncService).Verify(s => s.Save(metadata), Times.Exactly(2));
        }

        [TestMethod]
        public void RemovesFlashcards()
        {
            IFlashcardSet flashcardSet = MockFlashcardSet(new[] { flashcard1, flashcard2, flashcard3 });
            SyncMetadata metadata = new SyncMetadata
            {
                FlashcardIDs = new Dictionary<string, string> { { "p1", "f1" }, { "p2", "f2" }, { "p3", "f3" } },
                PetLastUpdateTimestamps = new Dictionary<string, DateTime>
                {
                    { "p1", DateTime.UtcNow },
                    { "p2", DateTime.UtcNow },
                    { "p3", DateTime.UtcNow }
                },
                PetIDs = new Dictionary<string, string> { { "f1", "p1" }, { "f2", "p2" }, { "f3", "p3" } }
            };
            ISyncService syncService = MockSyncService();
            Synchronizer synchronizer = MockSynchronizer(syncService);

            synchronizer.SynchronizeFlashcards(flashcardSet, new[] { pet2 }, metadata, options).Wait();

            Mock.Get(synchronizer).Verify(s => s.AddFlashcard(flashcardSet, It.IsAny<IPet>(), metadata, options), Times.Never);
            Mock.Get(synchronizer).Verify(s => s.UpdateFlashcard(flashcardSet, It.IsAny<IFlashcard>(), It.IsAny<IPet>(), metadata, options), Times.Never());
            Mock.Get(synchronizer).Verify(s => s.RemoveFlashcard(flashcardSet, flashcard1, metadata));
            Mock.Get(synchronizer).Verify(s => s.RemoveFlashcard(flashcardSet, flashcard2, metadata), Times.Never());
            Mock.Get(synchronizer).Verify(s => s.RemoveFlashcard(flashcardSet, flashcard3, metadata));
            Mock.Get(syncService).Verify(s => s.Save(metadata), Times.Exactly(2));
        }

        [TestMethod]
        public void UpdatesFlashcards()
        {
            IFlashcardSet flashcardSet = MockFlashcardSet(new[] { flashcard1, flashcard2, flashcard3 });
            SyncMetadata metadata = new SyncMetadata
            {
                FlashcardIDs = new Dictionary<string, string> { { "p1", "f1" }, { "p2", "f2" }, { "p3", "f3" } },
                PetLastUpdateTimestamps = new Dictionary<string, DateTime>
                {
                    { "p1", DateTime.UtcNow.AddMinutes(-1) },
                    { "p2", DateTime.UtcNow },
                    { "p3", DateTime.UtcNow.AddHours(-1) }
                },
                PetIDs = new Dictionary<string, string> { { "f1", "p1" }, { "f2", "p2" }, { "f3", "p3" } }
            };
            ISyncService syncService = MockSyncService();
            Synchronizer synchronizer = MockSynchronizer(syncService);

            synchronizer.SynchronizeFlashcards(flashcardSet, new[] { pet1, pet2, pet3 }, metadata, options).Wait();

            Mock.Get(synchronizer).Verify(s => s.AddFlashcard(flashcardSet, It.IsAny<IPet>(), metadata, options), Times.Never);
            Mock.Get(synchronizer).Verify(s => s.UpdateFlashcard(flashcardSet, flashcard1, It.IsAny<IPet>(), metadata, options));
            Mock.Get(synchronizer).Verify(s => s.UpdateFlashcard(flashcardSet, flashcard2, It.IsAny<IPet>(), metadata, options), Times.Never());
            Mock.Get(synchronizer).Verify(s => s.UpdateFlashcard(flashcardSet, flashcard3, It.IsAny<IPet>(), metadata, options));
            Mock.Get(synchronizer).Verify(s => s.RemoveFlashcard(flashcardSet, It.IsAny<IFlashcard>(), metadata), Times.Never());
            Mock.Get(syncService).Verify(s => s.Save(metadata), Times.Exactly(2));
        }
    }
}
