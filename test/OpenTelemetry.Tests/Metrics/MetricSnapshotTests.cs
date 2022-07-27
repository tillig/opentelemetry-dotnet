// <copyright file="MetricSnapshotTests.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.Metrics;

using OpenTelemetry.Tests;

using Xunit;

namespace OpenTelemetry.Metrics.Tests
{
    public class MetricSnapshotTests
    {
        [Fact]
        public void VerifySnapshot_Counter()
        {
            var exportedMetrics = new List<Metric>();
            var exportedSnapshots = new List<MetricSnapshot>();

            using var meter = new Meter(Utils.GetCurrentMethodName());
            var counter = meter.CreateCounter<long>("meter");
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(meter.Name)
                .AddInMemoryExporter(exportedMetrics)
                .AddInMemoryExporter(exportedSnapshots)
                .Build();

            // FIRST EXPORT
            counter.Add(10);
            meterProvider.ForceFlush();

            // Verify Metric 1
            Assert.Single(exportedMetrics);
            var metric1 = exportedMetrics[0];
            var metricPoints1Enumerator = metric1.GetMetricPoints().GetEnumerator();
            Assert.True(metricPoints1Enumerator.MoveNext());
            ref readonly var metricPoint1 = ref metricPoints1Enumerator.Current;
            Assert.Equal(10, metricPoint1.GetSumLong());

            // Verify Snapshot 1
            Assert.Single(exportedSnapshots);
            var snapshot1 = exportedSnapshots[0];
            Assert.Single(snapshot1.MetricPoints);
            Assert.Equal(10, snapshot1.MetricPoints[0].GetSumLong());

            // Verify Metric == Snapshot
            Assert.Equal(metric1.Name, snapshot1.Name);
            Assert.Equal(metric1.Description, snapshot1.Description);
            Assert.Equal(metric1.Unit, snapshot1.Unit);
            Assert.Equal(metric1.MeterName, snapshot1.MeterName);
            Assert.Equal(metric1.MetricType, snapshot1.MetricType);
            Assert.Equal(metric1.MeterVersion, snapshot1.MeterVersion);

            // SECOND EXPORT
            counter.Add(5);
            meterProvider.ForceFlush();

            // Verify Metric 1, after second export
            // This value is expected to be updated.
            Assert.Equal(15, metricPoint1.GetSumLong());

            // Verify Metric 2
            Assert.Equal(2, exportedMetrics.Count);
            var metric2 = exportedMetrics[1];
            var metricPoints2Enumerator = metric2.GetMetricPoints().GetEnumerator();
            Assert.True(metricPoints2Enumerator.MoveNext());
            ref readonly var metricPoint2 = ref metricPoints2Enumerator.Current;
            Assert.Equal(15, metricPoint2.GetSumLong());

            // Verify Snapshot 1, after second export
            // This value is expected to be unchanged.
            Assert.Equal(10, snapshot1.MetricPoints[0].GetSumLong());

            // Verify Snapshot 2
            Assert.Equal(2, exportedSnapshots.Count);
            var snapshot2 = exportedSnapshots[1];
            Assert.Single(snapshot2.MetricPoints);
            Assert.Equal(15, snapshot2.MetricPoints[0].GetSumLong());
        }

        [Fact]
        public void VerifySnapshot_Histogram()
        {
            var exportedMetrics = new List<Metric>();
            var exportedSnapshots = new List<MetricSnapshot>();

            using var meter = new Meter(Utils.GetCurrentMethodName());
            var histogram = meter.CreateHistogram<int>("histogram");
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(meter.Name)
                .AddInMemoryExporter(exportedMetrics)
                .AddInMemoryExporter(exportedSnapshots)
                .Build();

            // FIRST EXPORT
            histogram.Record(10);
            meterProvider.ForceFlush();

            // Verify Metric 1
            Assert.Single(exportedMetrics);
            var metric1 = exportedMetrics[0];
            var metricPoints1Enumerator = metric1.GetMetricPoints().GetEnumerator();
            Assert.True(metricPoints1Enumerator.MoveNext());
            ref readonly var metricPoint1 = ref metricPoints1Enumerator.Current;
            Assert.Equal(1, metricPoint1.GetHistogramCount());
            Assert.Equal(10, metricPoint1.GetHistogramSum());

            // Verify Snapshot 1
            Assert.Single(exportedSnapshots);
            var snapshot1 = exportedSnapshots[0];
            Assert.Single(snapshot1.MetricPoints);
            Assert.Equal(1, snapshot1.MetricPoints[0].GetHistogramCount());
            Assert.Equal(10, snapshot1.MetricPoints[0].GetHistogramSum());

            // Verify Metric == Snapshot
            Assert.Equal(metric1.Name, snapshot1.Name);
            Assert.Equal(metric1.Description, snapshot1.Description);
            Assert.Equal(metric1.Unit, snapshot1.Unit);
            Assert.Equal(metric1.MeterName, snapshot1.MeterName);
            Assert.Equal(metric1.MetricType, snapshot1.MetricType);
            Assert.Equal(metric1.MeterVersion, snapshot1.MeterVersion);

            // SECOND EXPORT
            histogram.Record(5);
            meterProvider.ForceFlush();

            // Verify Metric 1 after second export
            // This value is expected to be updated.
            Assert.Equal(2, metricPoint1.GetHistogramCount());
            Assert.Equal(15, metricPoint1.GetHistogramSum());

            // Verify Metric 2
            Assert.Equal(2, exportedMetrics.Count);
            var metric2 = exportedMetrics[1];
            var metricPoints2Enumerator = metric2.GetMetricPoints().GetEnumerator();
            Assert.True(metricPoints2Enumerator.MoveNext());
            ref readonly var metricPoint2 = ref metricPoints2Enumerator.Current;
            Assert.Equal(2, metricPoint2.GetHistogramCount());
            Assert.Equal(15, metricPoint2.GetHistogramSum());

            // Verify Snapshot 1 after second export
            // This value is expected to be unchanged.
            Assert.Equal(1, snapshot1.MetricPoints[0].GetHistogramCount());
            Assert.Equal(10, snapshot1.MetricPoints[0].GetHistogramSum());

            // Verify Snapshot 2
            Assert.Equal(2, exportedSnapshots.Count);
            var snapshot2 = exportedSnapshots[1];
            Assert.Single(snapshot2.MetricPoints);
            Assert.Equal(2, snapshot2.MetricPoints[0].GetHistogramCount());
            Assert.Equal(15, snapshot2.MetricPoints[0].GetHistogramSum());
        }
    }
}