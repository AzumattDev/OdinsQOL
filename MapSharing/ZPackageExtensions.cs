﻿using static VMP_Mod.VPlusDataObjects;

namespace VMP_Mod.Utility
{
    public static class ZPackageExtensions
    {
        public static MapRange ReadVPlusMapRange(this ZPackage pkg)
        {
            return new MapRange
            {
                StartingX = pkg.m_reader.ReadInt32(),
                EndingX = pkg.m_reader.ReadInt32(),
                Y = pkg.m_reader.ReadInt32()
            };
        }

        public static void WriteVPlusMapRange(this ZPackage pkg, MapRange mapRange)
        {
            pkg.m_writer.Write(mapRange.StartingX);
            pkg.m_writer.Write(mapRange.EndingX);
            pkg.m_writer.Write(mapRange.Y);
        }
    }
}