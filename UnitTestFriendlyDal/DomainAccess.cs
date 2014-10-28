using System;
using System.Linq;

using NHibernate.Linq;

namespace UnitTestFriendlyDal
{

    public interface IDomainAccessFactory
    {
        IDomainAccess OpenDomainAccess();
    }

    public class DomainAccessFactory : IDomainAccessFactory
    {
        NHibernate.ISessionFactory _sessionFactory;

        public DomainAccessFactory(NHibernate.ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public IDomainAccess OpenDomainAccess()
        {
            return new DomainAccess(_sessionFactory);
        }
    }


    public interface IDomainAccess : IDisposable
    {
        IQueryable<T> Query<T>();
        T Get<T>(object id);
        T Load<T>(object id);
        object Save(object transientObject);
        void Evict<T>(object id);
    }

    class DomainAccess : IDomainAccess
    {

        NHibernate.ISessionFactory _sessionFactory;
        NHibernate.ISession _session;
        NHibernate.ITransaction _transaction;




        public DomainAccess(NHibernate.ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
            _session = _sessionFactory.OpenSession();
            _transaction = _session.BeginTransaction();
        }


        IQueryable<T> IDomainAccess.Query<T>()
        {
            return _session.Query<T>();
        }


        T IDomainAccess.Get<T>(object id)
        {
            return _session.Get<T>(id);
        }

        T IDomainAccess.Load<T>(object id)
        {
            return _session.Load<T>(id);
        }



        void IDomainAccess.Evict<T>(object id)
        {
            _sessionFactory.Evict(typeof(T), id);
        }



        object IDomainAccess.Save(object transientObject)
        {
            return _session.Save(transientObject);
        }


        // Because transaction is a cross-cutting concern. It should be automated
        void IDisposable.Dispose()
        {
            // http://www.hibernatingrhinos.com/products/nhprof/learn/alert/donotuseimplicittransactions

            _transaction.Commit();
            _transaction.Dispose();
            _session.Dispose();
        }






    }


    public static class LinqExtensionMethods
    {
        public static IQueryable<T> GetPage<T>(this IQueryable<T> query, int pageLimit, int pageNumber)
        {
            var paged = query.Take(pageLimit).Skip(pageLimit * (pageNumber - 1));

            return paged;
        }
    }


    /// <summary>
    /// cross-cutting concern    
    /// MakeCacheable replaces Cacheable, so IQueryable detection provider can be done here
    /// Can't use NHibernate's built-in .Cacheable on non-NHibernate IQueryable, it will throw an error    
    /// </summary>
    public static class NHibernateLinqExtensionMethods
    {
        public static IQueryable<T> MakeCacheable<T>(this IQueryable<T> query)
        {
            if (query.Provider.GetType() == typeof(NHibernate.Linq.DefaultQueryProvider))
                query = query.Cacheable();

            return query;
        }


    }

}
