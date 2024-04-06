//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NeonShooter
{
    public class ParticleManager<T>
	{
		// This delegate will be called for each particle.
		private Action<Particle> updateParticle;
		private CircularParticleArray particleList;

		/// <summary>
		/// Allows creation of particles.
		/// </summary>
		/// <param name="capacity">The maximum number of particles. An array of this size will be pre-allocated.</param>
		/// <param name="updateParticle">A delegate that lets you specify custom behaviour for your particles. Called once per particle, per frame.</param>
		public ParticleManager(int capacity, Action<Particle> updateParticle)
		{
			this.updateParticle = updateParticle;
			particleList = new CircularParticleArray(capacity);

			// Populate the list with empty particle objects, for reuse.
			for (int i = 0; i < capacity; i++)
				particleList[i] = new Particle();
		}

		/// <summary>
		/// Update particle state, to be called every frame.
		/// </summary>
		public void Update()
		{
			int removalCount = 0;
			for (int i = 0; i < particleList.Count; i++)
			{
				var particle = particleList[i];

				updateParticle(particle);

				particle.PercentLife -= 1f / particle.Duration;

				// sift deleted particles to the end of the list
				Swap(particleList, i - removalCount, i);

				// if the alpha < 0, delete this particle
				if (particle.PercentLife < 0)
					removalCount++;
			}
			particleList.Count -= removalCount;
		}

		private static void Swap(CircularParticleArray list, int index1, int index2)
		{
			var temp = list[index1];
			list[index1] = list[index2];
			list[index2] = temp;
		}

		/// <summary>
		/// Draw the particles.
		/// </summary>
		public void Draw(SpriteBatch spriteBatch)
		{
			for (int i = 0; i < particleList.Count; i++)
			{
				var particle = particleList[i];

				Vector2 origin = new Vector2(particle.Texture.Width / 2, particle.Texture.Height / 2);
				spriteBatch.Draw(particle.Texture, particle.Position, null, particle.Tint, particle.Orientation, origin, particle.Scale, 0, 0);
			}
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, float scale, T state, float theta = 0)
		{
			CreateParticle(texture, position, tint, duration, new Vector2(scale), state, theta);
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale, T state, float theta = 0)
		{
			Particle particle;
			if (particleList.Count == particleList.Capacity)
			{
				// if the list is full, overwrite the oldest particle, and rotate the circular list
				particle = particleList[0];
				particleList.Start++;
			}
			else
			{
				particle = particleList[particleList.Count];
				particleList.Count++;
			}

			// Create the particle
			particle.Texture = texture;
			particle.Position = position;
			particle.Tint = tint;

			particle.Duration = duration;
			particle.PercentLife = 1f;
			particle.Scale = scale;
			particle.Orientation = theta;
			particle.State = state;
		}

		/// <summary>
		/// Destroys all particles
		/// </summary>
		public void Clear()
		{
			particleList.Count = 0;
		}

		public int ParticleCount
		{
			get { return particleList.Count; }
		}

		public class Particle
		{
			public Texture2D Texture;
			public Vector2 Position;
			public float Orientation;

			public Vector2 Scale = Vector2.One;

			public Color Tint;
			public float Duration;
			public float PercentLife = 1f;
			public T State;
		}

		// Represents a circular array with an arbitrary starting point. It is useful for efficiently overwriting
		// the oldest particles when the array gets full. Simply overwrite particleList[0] and advance Start.
		private class CircularParticleArray
		{
			private int start;
			public int Start
			{
				get { return start; }
				set { start = value % list.Length; }
			}

			public int Count { get; set; }
			public int Capacity { get { return list.Length; } }
			private Particle[] list;

			public CircularParticleArray() { }  // for serialization

			public CircularParticleArray(int capacity)
			{
				list = new Particle[capacity];
			}

			public Particle this[int i]
			{
				get { return list[(start + i) % list.Length]; }
				set { list[(start + i) % list.Length] = value; }
			}
		}
	}
}