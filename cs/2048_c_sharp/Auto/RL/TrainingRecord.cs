using System;
using System.Collections.Generic;
using System.Linq;

using LinqToDB.Mapping;
using LinqToDB;

namespace _2048_c_sharp.Auto
{
    /// <summary>
    /// Each record represents the expected reward for a given state (Id) for all directions
    /// </summary>
    [Table(Name = "training")]
    public sealed class Training
    {
        public Training() { }
        public Training(long id) => Id = id;
        public Training(long id, Direction direction, float reward) : this(id) => Update(direction, reward);

        [Column,     NotNull, PrimaryKey] public long Id { get; set; }
        [Column,     NotNull ] public byte[] Results { get; set; } = new byte[State.StateSize];

        public WeightData this[int direction]
        {
            get => new WeightData(Results, State.WeightSize * direction);
            set => Buffer.BlockCopy(value.ToByteArray(), 0, Results, State.WeightSize * direction, State.WeightSize);
        }

        public Dictionary<Direction, float> GetWeights
            => XT.EnumVals<Direction>().ToDictionary(d => d, d => this[(int)d].Weight);

        public Dictionary<Direction, WeightData> GetWeightData
            => XT.EnumVals<Direction>().ToDictionary(d => d, d => this[(int)d]);

        public void Update(Direction direction, float reward)
            => this[(int)direction] = new WeightData(this[(int)direction], reward);
    }

    /// <summary>
    /// One option for weight storage would be to use a single "State" blob field (byte[]) in the database. 16 bytes
    /// would be assigned to the counts (4 bytes per count), and 16 bytes would be assigned to the weights.
    /// </summary>

    public class WeightData
    {
        public readonly float Rewards;
        public readonly int Count;

        public float Weight => Count == 0 ? default : Rewards / Count;

        public byte[] ToByteArray()
        {
            var retVal = new byte[State.BYTES_PER_REWARD + State.BYTES_PER_COUNT];
            Buffer.BlockCopy(BitConverter.GetBytes(Count), 0, retVal, 0, State.BYTES_PER_COUNT);
            Buffer.BlockCopy(BitConverter.GetBytes(Rewards), 0, retVal, State.BYTES_PER_COUNT, State.BYTES_PER_REWARD);
            return retVal;
        }

        public WeightData(int count, float rewards)
        {
            Count = count;
            Rewards = rewards;
        }

        public WeightData(WeightData current, float newRewards)
        {
            Count = current.Count + 1;
            Rewards = current.Rewards + newRewards;
        }

        public WeightData(byte[] data, int offset = 0)
        {
            Count = BitConverter.ToInt32(data, offset);
            Rewards = BitConverter.ToSingle(data, offset + sizeof(float));
        }
    }

    public static class WeightDataExtensions
    {
        public static int GetRecordCount(this Dictionary<Direction, WeightData> data) => data.Sum(kvp => kvp.Value.Count);
        public static Direction GetRecDirection(this Dictionary<Direction, WeightData> data)
            => data.OrderByDescending(kvp => kvp.Value.Weight).First().Key;
    }

    public static class State
    {
        public const int BYTES_PER_COUNT = 4;
        public const int BYTES_PER_REWARD = 4;
        public const int DIRECTION_COUNT = 4;
        public static int StateSize => (BYTES_PER_COUNT + BYTES_PER_REWARD) * DIRECTION_COUNT;
        public static int WeightSize => BYTES_PER_REWARD + BYTES_PER_COUNT;
    }
}
