using System.Collections;

namespace ShareInvest.Catalog
{
    public struct 업종지수 : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            foreach (var index in new int[]
            {
                20,
                10,
                11,
                12,
                15,
                13,
                14,
                16,
                17,
                18,
                25,
                26
            })
                yield return index;
        }
    }
}