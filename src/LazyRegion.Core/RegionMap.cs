using System;
using System.Collections.Generic;

namespace LazyRegion.Core
{
    public static class RegionMap
    {
        // 리전 이름 -> 현재 등록된 뷰 키
        private static readonly Dictionary<string, object?> regionOfViews = new ();

        // 리전에 뷰 등록 (기존 뷰가 있으면 교체)
        public static object? Register(string regionName, object? viewKey)
        {
            if (string.IsNullOrWhiteSpace (regionName))
                throw new ArgumentException ("Region name cannot be null or empty.", nameof (regionName));

            // 기존에 등록된 뷰 키 반환 (없으면 null)
            var previousViewKey = regionOfViews.TryGetValue (regionName, out var oldViewKey) ? oldViewKey : null;

            // 새 뷰로 교체
            regionOfViews[regionName] = viewKey;

            return previousViewKey;
        }

        // 리전의 현재 뷰 가져오기
        public static object? GetCurrentView(string regionName)
        {
            return regionOfViews.TryGetValue (regionName, out var viewKey) ? viewKey : null;
        }

        // 리전에 뷰가 등록되어 있는지 확인
        public static bool HasView(string regionName)
        {
            return regionOfViews.ContainsKey (regionName);
        }

        // 리전의 뷰 해제
        public static object? Unregister(string regionName)
        {
            if (regionOfViews.TryGetValue (regionName, out var viewKey))
            {
                regionOfViews.Remove (regionName);
                return viewKey;
            }
            return null;
        }

        // 모든 리전 정보 초기화
        public static void Clear()
        {
            regionOfViews.Clear ();
        }

        // 등록된 리전 개수
        public static int Count => regionOfViews.Count;

        // 모든 리전 이름 가져오기
        public static IEnumerable<string> GetAllRegionNames()
        {
            return regionOfViews.Keys;
        }
    }
}
