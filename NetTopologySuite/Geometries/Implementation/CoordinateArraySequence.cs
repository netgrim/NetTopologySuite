using System;
using System.Text;
using GeoAPI.Geometries;
using System.ComponentModel;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A <see cref="ICoordinateSequence"/> backed by an array of <see cref="Coordinate"/>s.
    /// This is the implementation that <see cref="IGeometry"/>s use by default.
    /// <para/>
    /// Coordinates returned by <see cref="ToCoordinateArray"/>, <see cref="GetCoordinate(int)"/> and <see cref="GetCoordinate(int, Coordinate)"/> are live --
    /// modifications to them are actually changing the
    /// CoordinateSequence's underlying data.
    /// A dimension may be specified for the coordinates in the sequence,
    /// which may be 2 or 3.
    /// The actual coordinates will always have 3 ordinates,
    /// but the dimension is useful as metadata in some situations. 
    /// </summary>
#if !PCL
    [Serializable]
#endif
    public class CoordinateArraySequence : ICoordinateSequence
    {    
        private double[,] _coordinates;
  
        /// <summary>
        /// Constructs a sequence based on the given array of <see cref="Coordinate"/>s.
        /// </summary>
        /// <remarks>
        /// The array is copied.
        /// </remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        public CoordinateArraySequence(Coordinate[] coordinates) 
        {
            if (coordinates == null || coordinates.Length == 0)
                _coordinates = new double[0,0];
            else
            {
                var length = coordinates.Length;
                _ordinate = ToSupportedOrdinates(coordinates[0].Ordinates);

                var dimension = Dimension;
                _coordinates = new double[length, dimension];
                var ordinates = new double[dimension];

                for (int i = 0; i < length; i++)
                {
                    var coordinate = coordinates[i];
                    if(coordinate.Ordinates != Ordinates)
                        throw new TopologyException ("All coordinates must have the same ordinates", coordinate);

                    coordinates[i].GetOrdinates(ordinates);
                    for (int j = 0; j < dimension; j++)
                        _coordinates[i, j] = ordinates[j];
                }
            }
        }
        
        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public CoordinateArraySequence(int size)
            : this(size, Ordinates.XY)
        {
        }

        /// <summary>
        /// Constructs a sequence of a given <paramref name="size"/>, populated 
        /// with new <see cref="Coordinate"/>s of the given <paramref name="dimension"/>.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
		/// <param name="ordinates">the ordinates of the coordinates</param>
        public CoordinateArraySequence(int size, Ordinates ordinates)
        {
            _ordinate = ToSupportedOrdinates(ordinates);
            _coordinates = new double[size, Dimension];
        }


        /// <summary>
        /// Creates a new sequence based on a deep copy of the given <see cref="ICoordinateSequence"/>.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence that will be copied</param>
        public CoordinateArraySequence(ICoordinateSequence coordSeq)
        {
            if (coordSeq == null)
            {
                _ordinate = SupportedOrdinate.XY;
                _coordinates = new double[0,0];
                return;
            }

            var coordinateArraySeq = coordSeq as CoordinateArraySequence;
            if(coordinateArraySeq != null)
            {
                _ordinate = coordinateArraySeq._ordinate;
                _coordinates = (double[,])coordinateArraySeq._coordinates.Clone();
            }
            
            _ordinate = ToSupportedOrdinates(coordSeq.Ordinates);
            var dimension = Dimension;

            _coordinates = new double[coordSeq.Count, dimension];
            var ordinates = new double[dimension];
            var coordinate = new Coordinate(coordSeq.Ordinates);

            var length = coordSeq.Count;
            for (var i = 0; i < length; i++)
            {
                coordSeq.GetCoordinate(i, coordinate);
                coordinate.GetOrdinates(ordinates);

                for (int j = 0; j < dimension; j++)
                    _coordinates[i,j] = ordinates[j];
            }
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public int Dimension
        {
            get
            {
                switch (_ordinate)
                {
                    case SupportedOrdinate.XY:
                        return 2;
                    case SupportedOrdinate.XYM:
                    case SupportedOrdinate.XYZ:
                        return 3;
                    case SupportedOrdinate.XYZM:
                        return 4;
                    default:
                            throw new InvalidEnumArgumentException("Ordinates must represent 2,3 or 4 dimensions");
                    }
            }
        }

        private enum SupportedOrdinate
        {
            XY,
            XYM,
            XYZ,
            XYZM
        }

        private readonly SupportedOrdinate _ordinate;

        public Ordinates Ordinates
        {
            get
            {
                switch (_ordinate)
                {
                    case SupportedOrdinate.XY: return Ordinates.XY;
                    case SupportedOrdinate.XYM: return Ordinates.XYM;
                    case SupportedOrdinate.XYZ: return Ordinates.XYZ;
                    case SupportedOrdinate.XYZM: return Ordinates.XYZM;
                    default: throw new InvalidEnumArgumentException();
                }
            }
        }

        private SupportedOrdinate ToSupportedOrdinates(Ordinates ordinates)
        {
            switch (ordinates)
            {
                case Ordinates.XY: return SupportedOrdinate.XY;
                case Ordinates.XYM: return SupportedOrdinate.XYM;
                case Ordinates.XYZ: return SupportedOrdinate.XYZ;
                case Ordinates.XYZM: return SupportedOrdinate.XYZM;
                default: throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Get the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>The requested Coordinate instance.</returns>
        public Coordinate GetCoordinate(int i)
        {
            switch (_ordinate)
            {
                case SupportedOrdinate.XY:
                    return new Coordinate(_coordinates[i, 0], _coordinates[i, 1]);
                case SupportedOrdinate.XYM:
                    return Coordinate.FromXYM(_coordinates[i, 0], _coordinates[i, 1], _coordinates[i, 2]);
                case SupportedOrdinate.XYZ:
                    return new Coordinate(_coordinates[i, 0], _coordinates[i, 1], _coordinates[i, 2]);
                case SupportedOrdinate.XYZM:
                    return new Coordinate(_coordinates[i, 0], _coordinates[i, 1], _coordinates[i, 2], _coordinates[i, 3]);
                default:
                    throw new InvalidEnumArgumentException("Ordinates must represent 2,3 or 4 dimensions");
            }
        }
        /// <summary>
        /// Get a copy of the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>A copy of the requested Coordinate.</returns>
        public virtual Coordinate GetCoordinateCopy(int i) 
        {
            return GetCoordinate(i);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(int index, Coordinate coord)
        {
            if (coord.Ordinates != Ordinates)
                throw new ArgumentException("Ordinates must be the same");

            coord.X = _coordinates[index, 0];
            coord.Y = _coordinates[index, 1];

            switch (_ordinate)
            {
                case SupportedOrdinate.XYM:
                    coord.M = _coordinates[index, 2];
                    return;
                case SupportedOrdinate.XYZ:
                    coord.Z = _coordinates[index, 2];
                    return;
                case SupportedOrdinate.XYZM:
                    coord.M = _coordinates[index, 3];
                    goto case SupportedOrdinate.XYZ;
                case SupportedOrdinate.XY:
                    return;
                default:
                    throw new InvalidEnumArgumentException("Ordinates must represent 2,3 or 4 dimensions");
            }
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public double GetX(int index) 
        {
            return _coordinates[index, 0];
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public double GetY(int index) 
        {
            return _coordinates[index, 1];
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public double GetOrdinate(int index, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return _coordinates[index, 0];
                case Ordinate.Y:
                    return _coordinates[index, 1];
                case Ordinate.Z:
                    if ((Ordinates & Ordinates.Z) == Ordinates.Z)
                        return _coordinates[index, 2];
                break;
            case Ordinate.M:
                if (Ordinates == Ordinates.XYM)
                    return _coordinates[index, 2];
                else if (Ordinates == Ordinates.XYZM)
                    return _coordinates[index, 3];
                break;
            }
            throw new InvalidEnumArgumentException("Ordinates does not contains " + ordinate);
        }

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        /// <returns>The deep copy.</returns>
        public virtual object Clone()
        {
            return new CoordinateArraySequence(this);
        }

        /// <summary>
        /// Returns the length of the coordinate sequence.
        /// </summary>
        public int Count 
        {
            get
            {
                return _coordinates.GetLength(0);
            }
        }

        /// <summary>
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <param name="value">The new ordinate value.</param>
        public void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    _coordinates[index, 0] = value;
                    return;
                case Ordinate.Y:
                    _coordinates[index, 1] = value;
                    return;
                case Ordinate.Z:
                    if ((Ordinates & Ordinates.Z) == Ordinates.Z)
                        _coordinates[index, 2] = value;
                    return;
                case Ordinate.M:
                    if (Ordinates == Ordinates.XYM)
                        _coordinates[index, 2] = value;
                    else if (Ordinates == Ordinates.XYZM)
                        _coordinates[index, 3] = value;
                    return;
            }
            throw new InvalidEnumArgumentException("Ordinates does not contains " + ordinate);

        }

        /// <summary>
        /// Returns an array of Coordinate Objects.       
        /// </summary>
        /// <returns></returns>
        public Coordinate[] ToCoordinateArray() 
        {
            var count = Count;
            var coordinates = new Coordinate[count];
            for (int i = 0; i < count; i++)
                coordinates[i] = GetCoordinate(i);
            return coordinates;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public Envelope ExpandEnvelope(Envelope env)
        {
            var count = Count;
            for (int i = 0; i < count; i++ ) 
                env.ExpandToInclude(GetCoordinate(i));            
            return env;
        }

        public ICoordinateSequence Reversed()
        {
            var coordinates = new Coordinate[Count];
            for (var i = 0; i < Count; i++ )
            {
                coordinates[Count - i - 1] = new Coordinate(GetCoordinate(i));
            }
            return new CoordinateArraySequence(coordinates);
        }

        /// <summary>
        /// Returns the string representation of the coordinate array.
        /// </summary>
        /// <returns></returns>
        public override string ToString() 
        {
            if (Count> 0) 
            {
                StringBuilder strBuf = new StringBuilder(17 * Count);
                strBuf.Append('(');
                strBuf.Append(GetCoordinate(0));
                for (int i = 1; i < Count; i++) 
                {
                    strBuf.Append(", ");
                    strBuf.Append(GetCoordinate(i));
                }
                strBuf.Append(')');
                return strBuf.ToString();
            } 
            else return "()";
        }
    }
}
