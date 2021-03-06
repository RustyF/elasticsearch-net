﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Nest;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;
using Tests.Framework.MockData;
using static Nest.Infer;

namespace Tests.Aggregations.Metric.PercentileRanks
{
	public class PercentileRanksAggregationUsageTests : AggregationUsageTestBase
	{
		public PercentileRanksAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			commits_outlier = new
			{
				percentile_ranks = new
				{
					field = "numberOfCommits",
					values = new[] {15.0, 30.0},
					tdigest = new
					{
						compression = 200.0
					},
					script = new
					{
						source = "doc['numberOfCommits'].value * 1.2",
					},
					missing = 0.0
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.PercentileRanks("commits_outlier", pr => pr
				.Field(p => p.NumberOfCommits)
				.Values(15, 30)
				.Method(m => m
					.TDigest(td => td
						.Compression(200)
					)
				)
				.Script(ss => ss.Source("doc['numberOfCommits'].value * 1.2"))
				.Missing(0)
			);

		protected override AggregationDictionary InitializerAggs =>
			new PercentileRanksAggregation("commits_outlier", Field<Project>(p => p.NumberOfCommits))
			{
				Values = new List<double> {15, 30},
				Method = new TDigestMethod
				{
					Compression = 200
				},
				Script = new InlineScript("doc['numberOfCommits'].value * 1.2"),
				Missing = 0
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();
			var commitsOutlier = response.Aggregations.PercentileRanks("commits_outlier");
			commitsOutlier.Should().NotBeNull();
			commitsOutlier.Items.Should().NotBeNullOrEmpty();
			foreach (var item in commitsOutlier.Items)
				item.Should().NotBeNull();
		}
	}
}
