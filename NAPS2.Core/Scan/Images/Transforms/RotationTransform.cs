﻿using System;
using System.Drawing;

namespace NAPS2.Scan.Images.Transforms
{
    public class RotationTransform : Transform
    {
        private const double TOLERANCE = 0.001;

        public static double NormalizeAngle(double angle)
        {
            var mod = angle % 360.0;
            if (mod < 0)
            {
                mod += 360.0;
            }
            return mod;
        }

        private double angle;

        public RotationTransform()
        {
        }

        public RotationTransform(RotateFlipType rotateFlipType)
        {
            switch (rotateFlipType)
            {
                case RotateFlipType.Rotate90FlipNone:
                    Angle = 90.0;
                    break;

                case RotateFlipType.Rotate180FlipNone:
                    Angle = 180.0;
                    break;

                case RotateFlipType.Rotate270FlipNone:
                    Angle = 270.0;
                    break;

                case RotateFlipType.RotateNoneFlipNone:
                    Angle = 0.0;
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        public RotationTransform(double angle)
        {
            Angle = angle;
        }

        public double Angle
        {
            get { return angle; }
            set { angle = NormalizeAngle(value); }
        }

        public override Bitmap Perform(Bitmap bitmap)
        {
            if (Math.Abs(Angle - 0.0) < TOLERANCE)
            {
                return bitmap;
            }
            if (Math.Abs(Angle - 90.0) < TOLERANCE)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                return bitmap;
            }
            if (Math.Abs(Angle - 180.0) < TOLERANCE)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                return bitmap;
            }
            if (Math.Abs(Angle - 270.0) < TOLERANCE)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                return bitmap;
            }
            Bitmap result;
            if (Angle > 45.0 && Angle < 135.0 || Angle > 225.0 && Angle < 315.0)
            {
                result = new Bitmap(bitmap.Height, bitmap.Width);
            }
            else
            {
                result = new Bitmap(bitmap.Width, bitmap.Height);
            }
            using (var g = Graphics.FromImage(result))
            {
                // TODO: Background (maybe)
                g.TranslateTransform(result.Width / 2.0f, result.Height / 2.0f);
                g.RotateTransform((float)Angle);
                g.TranslateTransform(-bitmap.Width / 2.0f, -bitmap.Height / 2.0f);
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }
            bitmap.Dispose();
            return result;
        }

        public override bool CanSimplify(Transform other)
        {
            return other is RotationTransform;
        }

        public override Transform Simplify(Transform other)
        {
            var other2 = (RotationTransform)other;
            return new RotationTransform(Angle + other2.Angle);
        }

        public override bool IsNull
        {
            get { return Math.Abs(Angle - 0.0) < TOLERANCE; }
        }
    }
}