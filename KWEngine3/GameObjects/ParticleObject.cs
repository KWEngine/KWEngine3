using KWEngine3.Audio.CSVorbis;
using KWEngine3.Helper;
using KWEngine3.Model;
using OpenTK.Mathematics;
using System.Data.Common;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Partikelklasse
    /// </summary>
    public class ParticleObject
    {
        /// <summary>
        ///  Konstruktormethode für Partikel mit benutzerdefininiertem Partikel-Spritesheet
        /// </summary>
        /// <param name="scale">Skalierung</param>
        /// <param name="texture">zu verwendende Spritesheet-Textur</param>
        /// <param name="columns">Anzahl der Spalten im Spritesheet</param>
        /// <param name="rows">Anzahl der Zeilen im Spritesheet</param>
        /// <param name="isLooping">Ankerposition der Partikelanimation</param>
        /// <param name="speed">Geschwindigkeit der Partikelanimation</param>
        /// /// <param name="anchor">Ankerposition der Partikelanimation</param>
        public ParticleObject(float scale, string texture, int columns, int rows, bool isLooping, ParticleObjectSpeed speed = ParticleObjectSpeed.FPS060, ParticleObjectAnchor anchor = ParticleObjectAnchor.Center)
        {
            _q = new FXQuad(texture, columns, rows, anchor);
            _q.SetSpriteSheetLooping(isLooping);
            _q.SetSpriteSheetSpeed(speed);
            _q.SetScale(scale);
        }

        /// <summary>
        /// Konstruktormethode für Partikel und engine-interner Partikelanimation
        /// </summary>
        /// <param name="scale">Skalierung</param>
        /// <param name="type">Art der Partikelanimation</param>
        /// <param name="speed">Geschwindigkeit der Partikelanimation</param>
        /// <param name="anchor">Ankerposition der Partikelanimation</param>
        public ParticleObject(float scale, ParticleType type, ParticleObjectSpeed speed = ParticleObjectSpeed.FPS060, ParticleObjectAnchor anchor = ParticleObjectAnchor.Center)
        {
            _q = new FXQuad(type, ParticleObjectAnchor.Center);
            _q.SetSpriteSheetSpeed(speed);
            _q.SetScale(scale);
        }

        /// <summary>
        /// Gibt an, ob die Instanz aktuell noch wiedergegeben wird
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _q != null && !_q.SkipRender;
            }
        }

        /// <summary>
        /// Setzt die Dauer der Loop-Partikel
        /// </summary>
        /// <param name="durationInSeconds">Dauer (in Sekunden)</param>
        public void SetDuration(float durationInSeconds)
        {
            _q.SetDuration(durationInSeconds);
        }

        /// <summary>
        /// Setzt, wie stark die Pixelfarben des Partikeleffekts verstärkt werden
        /// </summary>
        /// <param name="intensity">Verstärkungswert (zwischen 0 und 10; Standardwert: 0f)</param>
        public void SetColorIntensity(float intensity)
        {
            _q.SetColorEmissive(0f, 0f, 0f, intensity);
        }

        /// <summary>
        /// Setzt die Farbverschiebung (Hue) in Grad
        /// </summary>
        /// <param name="hue">Farbverschiebung (in Grad)</param>
        public void SetHue(float hue)
        {
            _q.SetHue(hue);
        }

        /// <summary>
        /// Erfragt die aktuelle Farbverschiebung (Hue) in Grad
        /// </summary>
        /// <returns>Aktuelle Farbverschiebung (in Grad)</returns>
        public float GetHue()
        {
            return MathHelper.RadiansToDegrees(_q._hues[0]);
        }

        /// <summary>
        /// Setzt die Positon
        /// </summary>
        /// <param name="pos">Positionsdaten</param>
        public void SetPosition(Vector3 pos)
        {
            _q.SetPosition(pos);
        }

        /// <summary>
        /// Setzt die Position
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetPosition(float x, float y, float z)
        {
            _q.SetPosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Partikelfärbung
        /// </summary>
        /// <param name="red">Rot (0 bis 1)</param>
        /// <param name="green">Grün (0 bis 1)</param>
        /// <param name="blue">Blau (0 bis 1)</param>
        /// <param name="alpha">Transparenz (0 bis 1)</param>
        public void SetColor(float red, float green, float blue, float alpha)
        {
            _q.SetColor(red, green, blue);
            _q.SetOpacity(alpha);
           
        }

        /// <summary>
        /// Setzt die Rotation der Instanz um die Kamerasichtachse
        /// </summary>
        /// <param name="angle">Rotationswinkel (in Grad)</param>
        public void SetRotation(float angle)
        {
            _q.SetRotationAfterTurn(angle);
        }

        /// <summary>
        /// Erfragt die aktuelle Färbung des Partikelobjekts
        /// </summary>
        public Vector3 Color
        {
            get { return _q.Color; }
        }

        #region Internals
        internal FXQuad _q;
        #endregion
    }
}
