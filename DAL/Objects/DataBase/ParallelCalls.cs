using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DAL
{
    public static partial class Database
    {
            /// <summary>
            /// Executes a database command asynchronously, streaming the results as an observable sequence as they become available.
            /// Need to use NuGet to add in refrences to Reactives libraries.
            /// </summary>
            /// <param name="command">The command to execute on each server.</param>
            /// <param name="connectionString">The connection string for the database on which <paramref name="command"/> should be executed.</param>
            /// <param name="recordParser">A method that transforms (i.e. parses) each SqlDataReader record retrieved by <paramref name="command"/> into a strongly-typed <typeparamref name="TQueryResult"/>. </param>
            /// <typeparam name="TQueryRecord">The type that is returned by the query.</typeparam>
            /// <returns>An observable sequence of <typeparamref name="TQueryResult"/> instances that are streamed from the server executing <paramref name="command"/>.</returns>
            public static IObservable<T> StreamFromServer<T>(SqlCommand command, String connectionString) where T : class, new()
            {
                // Create a disposable instance that supports cancellation to interact with the TPL properly.
                CancellationDisposable cancel = new CancellationDisposable();

                // Capture the action that we wish to carry out using a lambda, which effectively hides the async operation from the Observable.
                Func<IObserver<T>, Task> streamResults = async (observer) =>
                {
                    try
                    {
                        // Create the connection to the database.
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            // Associate the command with the connection, opening the connection.
                            command.Connection = connection;
                            await connection.OpenAsync().ConfigureAwait(false);

                            // Execute the command, reading each row asynchronously.
                            using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancel.Token).ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync(cancel.Token).ConfigureAwait(false))
                                {
                                    // Parse the row, providing it to the observer.
                                    observer.OnNext(ParseDatareaderResult<T>(reader));
                                }
                            }
                        }
                    }
                    catch (Exception error)
                    {
                        // Pass the exception into the observer for propagation by subscribers.
                        observer.OnError(error);
                    }

                    // We've completed successfully, so we indicate the end of the sequence.
                    observer.OnCompleted();
                };

                // Return the Observable that will start the process upon subscription.
                return Observable.Create<T>(observer =>
                {
                    // Use the captured method to start streaming data.
                    streamResults(observer);

                    // Return the disposable.
                    return cancel;
                });
            }
    }
}
