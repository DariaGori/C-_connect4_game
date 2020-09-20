using System;
using System.Runtime.Serialization;

namespace BLL
{
    [Serializable]
    public enum CellState
    {
        [EnumMember]
        Empty = 0,
        [EnumMember]
        WhiteBall = 1,
        [EnumMember]
        BlackBall = 2
    }
}