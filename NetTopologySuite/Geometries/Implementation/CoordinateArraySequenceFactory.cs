using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries.Implementation
{
    using System;
    using GeoAPI.Geometries;

    /// <summary>
    /// Creates CoordinateSequences represented as an array of Coordinates.
    /// </summary>
#if !PCL
    [Serializable]
#endif
    public sealed class CoordinateArraySequenceFactory : ICoordinateSequenceFactory
    {
        private static readonly CoordinateArraySequenceFactory instance = new CoordinateArraySequenceFactory();
        
        private CoordinateArraySequenceFactory() { }

        /// <summary>
        /// Returns the singleton instance of CoordinateArraySequenceFactory.
        /// </summary>
        public static CoordinateArraySequenceFactory Instance
        {
            get { return instance; }
        }

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        /// <returns></returns>
        public ICoordinateSequence Create(Coordinate[] coordinates) 
        {
            return new CoordinateArraySequence(coordinates);
        }

        public ICoordinateSequence Create(ICoordinateSequence coordSeq) 
        {
            return new CoordinateArraySequence(coordSeq);
        }
        
        public ICoordinateSequence Create(int size, int dimension)
        {
            switch (dimension)
            {
                case 2: return Create(size, Ordinates.XY);
                case 3: return Create(size, Ordinates.XYZ);
                case 4: return Create(size, Ordinates.XYZM);
                default:
                throw new ArgumentOutOfRangeException("dimension", "Valid values are 2, 3 or 4 dimensions");
            }
        }

        public ICoordinateSequence Create(int size, Ordinates ordinates)
        {
            return new CoordinateArraySequence(size, ordinates);
        }

        public Ordinates Ordinates
        {
            get { return Ordinates.XYZ; }
        }
    }
}
