using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods.Extension_Methods {
    public static class Extension_Methods {

        public static bool OutComponent<T>(this Component behavior, out T o) where T : class {
            o = (behavior.GetComponent(typeof(T)) as T);
            return o != null;
        }

        public static T RandomElement<T>(this HashSet<T> array) {
            return array.ElementAt(UnityEngine.Random.Range(0, array.Count));
        }

        public static T RandomElement<T>(this T[] array) {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static T RandomElement<T>(this List<T> array) {
            return array[UnityEngine.Random.Range(0, array.Count)];
        }

        public static T WeightedRandomElement<T>(this IEnumerable<T> source, Func<T, float> predicate) {
            List<KeyValuePair<T, float>> source2 = (from element in source
                                                    select new KeyValuePair<T, float>(element, Mathf.Max(predicate(element), 0f))).ToList();
            float pick = source2.Sum(element => element.Value) * UnityEngine.Random.value;
            return source2.SkipWhile(element => (pick -= element.Value) > 0f).First().Key;
        }
    }
}
