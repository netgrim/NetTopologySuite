using GeoAPI.Geometries;
using System;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Computes the <see cref="LinearLocation" /> for a given measure
    /// along a linear <see cref="Geometry" />
    /// Out-of-range values are clamped.
    /// </summary>
    public class MeasureLocationMap
    {
        /// <summary>
        /// Computes the <see cref="LinearLocation" /> for a
        /// given measure along a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="measure">The measure index of the location.</param>
        /// <returns>The <see cref="LinearLocation" /> for the length.</returns>
        public static LinearLocation GetLocation(IGeometry linearGeom, double measure)
        {
            var locater = new MeasureLocationMap(linearGeom);
            return locater.GetLocation(measure);
        }

        /// <summary>
        /// Computes the <see cref="LinearLocation"/> for a
        /// given measure along a linear <see cref="IGeometry"/>,
        /// with control over how the location
        /// is resolved at component endpoints.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use</param>
        /// <param name="measure">The measure index of the location</param>
        /// <param name="resolveLower">If true measureS are resolved to the lowest possible index</param>
        public static LinearLocation GetLocation(IGeometry linearGeom, double measure, bool resolveLower)
        {
            var locater = new MeasureLocationMap(linearGeom);
            return locater.GetLocation(measure, resolveLower);
        }

        /// <summary>
        /// Computes the measure for a given <see cref="LinearLocation" />
        /// on a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="loc">The <see cref="LinearLocation" /> index of the location.</param>
        /// <returns>The measure for the <see cref="LinearLocation" />.</returns>
        public static double GetMeasure(IGeometry linearGeom, LinearLocation loc)
        {
            var locater = new MeasureLocationMap(linearGeom);
            return locater.GetMeasure(loc);
        }

        private readonly IGeometry _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthLocationMap"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public MeasureLocationMap(IGeometry linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Compute the <see cref="LinearLocation"/> corresponding to a measure.
        /// Ambiguous indexes are resolved to the lowest or highest possible location value,
        /// depending on the value of <tt>resolveLower</tt>
        /// </summary>
        /// <param name="measure">The measure index</param>
        /// <param name="resolveLower"></param>
        /// <returns>The corresponding <see cref="LinearLocation" />.</returns>
        public LinearLocation GetLocation(double measure, bool resolveLower = true)
        {
            var firstComponent = (ILineString)_linearGeom.GetGeometryN(0);
            var startMeasure = firstComponent.GetCoordinateN(0).M;
            var numGeometries = _linearGeom.NumGeometries;
            var lastComponent = numGeometries > 1 ? (ILineString)_linearGeom.GetGeometryN(numGeometries - 1) : firstComponent;
            var endMeasure = lastComponent.GetCoordinateN(lastComponent.NumPoints-1).M;

            if (startMeasure == Coordinate.NullOrdinate || endMeasure == Coordinate.NullOrdinate)
                throw new InvalidLrsGeometry();
            if (startMeasure < endMeasure) //increasing
            {
                if (measure < startMeasure || measure > endMeasure ) throw new InvalidMeasureException();
                return GetLocationAscending(measure);
            }
            else //decreasing
            {
                if (measure > startMeasure || measure < endMeasure) throw new InvalidMeasureException();
                return GetLocationDescending(measure);
            }
        }

        private LinearLocation GetLocationDescending(double measure)
        {
            throw new NotImplementedException();
        }

        private LinearLocation GetLocationAscending(double measure)
        {

            var lo = 0;
            int hi = _linearGeom.NumGeometries - 1;
            while (lo <= hi)
            {
                int median = lo + ((hi - lo) >> 1);
                var medianGeom = (ILineString)_linearGeom.GetGeometryN(median);

                double startMeasure, endMeasure;

                if (measure < (startMeasure = medianGeom.GetCoordinateN(0).M))
                    hi = median - 1;
                else if (median > (endMeasure = medianGeom.GetCoordinateN(medianGeom.NumPoints - 1).M))
                    lo = median + 1;
                else if (startMeasure >= endMeasure)
                    throw new InvalidLrsGeometry();
                else
                    return GetLocationAscending(median, medianGeom, measure);
            }
            throw new InvalidMeasureException();
        }

        private LinearLocation GetLocationAscending(int componentIndex, ILineString line, double measure)
        {
            var lo = 0;
            int hi = line.NumPoints- 1;

            while (hi - lo > 1)
            {
                int median = lo + ((hi - lo) >> 1);
                var medianMeasure = line.GetCoordinateN(median).M;

                var cmp = measure.CompareTo(medianMeasure);

                if (cmp > 0)
                    lo = median;
                else
                    hi = median;
            }

            var loMeasure = line.GetCoordinateN(lo).M;
            var hiMeasure = line.GetCoordinateN(hi).M;

            return new LinearLocation(componentIndex, lo, (measure - loMeasure) / (hiMeasure - loMeasure));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public double GetMeasure(LinearLocation loc)
        {
            throw new NotImplementedException();
        }
    }
}