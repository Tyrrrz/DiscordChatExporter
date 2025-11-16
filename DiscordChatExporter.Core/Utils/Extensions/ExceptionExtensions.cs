using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class ExceptionExtensions
{
    extension(Exception exception)
    {
        private void PopulateChildren(ICollection<Exception> children)
        {
            if (exception is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    children.Add(innerException);
                    PopulateChildren(innerException, children);
                }
            }
            else if (exception.InnerException is not null)
            {
                children.Add(exception.InnerException);
                PopulateChildren(exception.InnerException, children);
            }
        }

        public IReadOnlyList<Exception> GetSelfAndChildren()
        {
            var children = new List<Exception> { exception };
            PopulateChildren(exception, children);
            return children;
        }
    }
}
