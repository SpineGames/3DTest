#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Particle3DSample
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class SmokePlumeParticleSystem : ParticleSystem
    {
        public SmokePlumeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }
        
        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 600;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.MinHorizontalVelocity = -2;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = -2;
            settings.MaxVerticalVelocity = 2;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(0, 0, 2);

            settings.MinColor = Color.FromNonPremultiplied(128, 128, 128, 255);
            settings.MaxColor = Color.FromNonPremultiplied(128, 128, 128, 255);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 6;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 4;
        }
    }
}
