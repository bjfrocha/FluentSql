﻿using FluentSql.Mappers;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSelectQuery<T> : SelectQuery<T>, IQuery<T>
    {

        #region Constructor
        public SqlServerSelectQuery() : base()
        { }
        #endregion

        #region Overrides

        public override IQuery<T> GetTopRows(int topNumberRows)
        {
            TopRows = topNumberRows;

            return this;
        }

        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var selectFields = new List<string>();
            var sqlJoinBuilder = new StringBuilder();

            selectFields = SqlServerHelper.BraketFieldNames(Fields.Select(f => f.ColumnName), TableAlias).ToList();

            foreach (var join in Joins)
            {
                var rightTableAlias = join.RightQuery.TableAlias;

                foreach (var field in join.RightQuery.Fields)
                {
                    selectFields.Add(string.Format("{0}.[{1}]", rightTableAlias, field.ColumnName));
                }

                sqlJoinBuilder.Append(join.ToSql());
            }

            if (TopRows > 0)
                sqlBuilder.Append(string.Format("{0} {1} {2} {3} ", Verb, EntityMapper.SqlGenerator.Top, TopRows, string.Join(",", selectFields)));
            else
                sqlBuilder.Append(string.Format("{0} {1} ", Verb, string.Join(",", selectFields)));

            if (EntityMapper.SqlGenerator.IncludeDbNameInQuery)
                sqlBuilder.Append(string.Format("FROM [{0}].[{1}].[{2}] {3} ", DatabaseName, SchemaName, TableName, TableAlias));
            else
                sqlBuilder.Append(string.Format("FROM [{0}].[{1}] {2} ", SchemaName, TableName, TableAlias));

            sqlBuilder.Append(sqlJoinBuilder.ToString());

            if (PredicateParts != null && PredicateParts.Any())
                sqlBuilder.Append(string.Format("WHERE {0} ", PredicateParts.ToSql()));
            else if (Predicate != null)
                sqlBuilder.Append(string.Format("WHERE {0} ", Predicate.ToSql()));

            if (OrderByFields != null)
                sqlBuilder.Append(string.Format("ORDER BY {0}", string.Join(",", OrderByFields.Select(f => f.ToSql()))));

            return sqlBuilder.ToString();
        }
        #endregion
    }
}
