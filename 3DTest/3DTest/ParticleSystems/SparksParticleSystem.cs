#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
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
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class SparksParticleSystem : ParticleSystem
    {
        public SparksParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "sparks";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(1F);
            settings.DurationRandomness = 0.5F;

            settings.MinHorizontalVelocity = -4;
            settings.MaxHorizontalVelocity = 4;

            settings.MinVerticalVelocity = -4;
            settings.MaxVerticalVelocity = 4;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkBlue;
            settings.MaxColor = Color.Aqua;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 2;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 13;
            settings.MaxEndSize = 20;

            // Use additive blending.
            settings.BlendState = BlendState.Opaque;
        }
    }
}
