﻿using System;
using System.Linq;
using System.Data.Entity;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class EntityWorkItemRepositoryFacts
    {
        public class TheGetActiveWorkerProperty
        {
            [Fact]
            public void ReturnsActiveWorkItemWhenAWorkerHasNoCompletedDate()
            {
                var workItems = new InMemoryDbSet<WorkItem> 
                {
                    new WorkItem
                    {
                        JobName = "docoolstuffjobname", 
                        Started = DateTime.UtcNow, 
                        Completed = null
                    }
                };
                var context = new Mock<WorkItemsContext>();
                context.Object.WorkItems = workItems;
                var repository = new EntityWorkItemRepository(() => context.Object);

                var activeWorkItem = repository.GetActiveWorker("docoolstuffjobname");

                Assert.NotNull(activeWorkItem);
            }

            [Fact]
            public void ReturnsNullWhenAllWorkItemsHaveCompletedDate()
            {
                var workItems = new InMemoryDbSet<WorkItem> 
                {
                    new WorkItem
                    {
                        JobName = "docoolstuff", 
                        Started = DateTime.UtcNow, 
                        Completed = DateTime.UtcNow
                    }
                };
                var context = new Mock<WorkItemsContext>();
                context.Object.WorkItems = workItems;
                var repository = new EntityWorkItemRepository(() => context.Object);

                var activeWorker = repository.GetActiveWorker("docoolstuff");

                Assert.Null(activeWorker);
            }

            [Fact]
            public void ReturnsNullWhenNoWorkItemsReturnedForGivenJob()
            {
                var workItems = new InMemoryDbSet<WorkItem> 
                {
                    new WorkItem
                    {
                        JobName = "douncoolstuff", 
                        Started = DateTime.UtcNow, 
                        Completed = DateTime.UtcNow
                    }
                };
                var context = new Mock<WorkItemsContext>();
                context.Object.WorkItems = workItems;
                var repository = new EntityWorkItemRepository(() => context.Object);

                var activeWorkItem = repository.GetActiveWorker("docoolstuff");

                Assert.Null(activeWorkItem);
            }
        }

        public class TheCreateWorkItemMethod
        {
            public void CreatesNewWorkItemThatIsNotComplete()
            {
                var before = DateTime.UtcNow;
                var context = new Mock<WorkItemsContext>();
                context.Setup(c => c.SaveChanges()).Verifiable();
                context.Object.WorkItems = new InMemoryDbSet<WorkItem>();
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.CreateWorkItem("web-server-1", "do-cool-stuff");

                var created = context.Object.WorkItems.First(w => w.WorkerId == "web-server-1");
                Assert.NotNull(created);
                Assert.Null(created.Completed);
                Assert.Equal("do-cool-stuff", created.JobName);
                Assert.True(created.Started >= before);
                context.Verify();
            }
        }

        public class TheSetWorkItemCompleteMethod
        {
            [Fact]
            public void SetsWorkItemCompletedSetsCompletedDate()
            {
                const long workItemId = 123;
                var context = new Mock<WorkItemsContext>();
                var workItem = new WorkItem { Id = workItemId };
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(workItemId)).Returns(workItem);
                context.Object.WorkItems = workItems.Object;
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemCompleted(workItemId);

                Assert.NotNull(workItem.Completed);
            }

            [Fact]
            public void SavesChanges()
            {
                var context = new Mock<WorkItemsContext>();
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(It.IsAny<long>())).Returns(new WorkItem());
                context.Object.WorkItems = workItems.Object;
                context.Setup(c => c.SaveChanges()).Verifiable();
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemCompleted(123);

                context.Verify();
            }
        }

        public class TheSetWorkItemFailedMethod
        {
            [Fact]
            public void SetsWorkItemFailedSetsCompletedDateAndExceptionInfo()
            {
                const long workItemId = 123;
                var context = new Mock<WorkItemsContext>();
                var workItem = new WorkItem { Id = workItemId };
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(workItemId)).Returns(workItem);
                context.Object.WorkItems = workItems.Object;
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemFailed(workItemId, new InvalidOperationException("Pretend failure!"));

                Assert.NotNull(workItem.Completed);
                Assert.Contains("Pretend failure!", workItem.ExceptionInfo);
            }

            [Fact]
            public void SavesChanges()
            {
                var context = new Mock<WorkItemsContext>();
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(It.IsAny<long>())).Returns(new WorkItem());
                context.Object.WorkItems = workItems.Object;
                context.Setup(c => c.SaveChanges()).Verifiable();
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemFailed(123, new InvalidOperationException());

                context.Verify();
            }
        }
    }
}
