﻿using System;
using AGS.API;
using System.Threading.Tasks;

namespace AGS.Engine
{
	public class ALAudioClip : IAudioClip
	{
		private ISoundData _soundData;
		private Lazy<int> _buffer;
		private IAudioSystem _system;
		private IAudioErrors _errors;
        private volatile int _numPlayingSounds;
        private IAudioBackend _backend;

        public ALAudioClip(string id, ISoundData soundData, IAudioSystem system, IAudioErrors errors, IAudioBackend backend)
		{
			_soundData = soundData;
			_errors = errors;
            _backend = backend;
			ID = id;
			_buffer = new Lazy<int> (() => generateBuffer());
			_system = system;
			Volume = 1f;
			Pitch = 1f;
		}

		#region ISound implementation

		public ISound Play(bool shouldLoop = false, ISoundProperties properties = null)
		{
			properties = properties ?? this;
			return playSound(properties.Volume, properties.Pitch, properties.Panning, shouldLoop);
		}

		public ISound Play(float volume, bool shouldLoop = false)
		{
			return playSound(volume, Pitch, Panning, shouldLoop);
		}

		public void PlayAndWait(ISoundProperties properties = null)
		{
			properties = properties ?? this;
			ISound sound = playSound(properties.Volume, properties.Pitch, properties.Panning);
			Task.Run(async () => await sound.Completed).Wait();
		}

		public void PlayAndWait(float volume)
		{
			ISound sound = playSound(volume, Pitch, Panning);
			Task.Run(async () => await sound.Completed).Wait();
		}
			
		public string ID { get; private set; }

		public float Volume { get; set; }
		public float Pitch { get; set; }
		public float Panning { get; set; }

        public bool IsPlaying { get { return _numPlayingSounds > 0; } }
        public int NumOfCurrentlyPlayingSounds { get { return _numPlayingSounds; } }

		#endregion

		private ISound playSound(float volume, float pitch, float panning, bool looping = false)
		{
            //Debug.WriteLine("Playing Sound: " + ID);
            _numPlayingSounds++;
			int source = getSource();
            ALSound sound = new ALSound (source, volume, pitch, looping, panning, _errors, _backend);
			sound.Play(_buffer.Value);
            sound.Completed.ContinueWith(_ =>
            {
                _system.ReleaseSource(source);
                _numPlayingSounds--;
            });
			return sound;
		}

		private int getSource()
		{
			return _system.AcquireSource();
		}

		private int generateBuffer()
		{
			_errors.HasErrors();

            int buffer = _backend.GenBuffer();
			Type dataType = _soundData.Data.GetType();
			if (dataType == typeof(byte[]))
			{
				byte[] bytes = (byte[])_soundData.Data;
                _backend.BufferData(buffer, getSoundFormat(_soundData.Channels, _soundData.BitsPerSample),
					bytes, _soundData.DataLength, _soundData.SampleRate);
			}
			else if (dataType == typeof(short[]))
			{
				short[] shorts = (short[])_soundData.Data;
                _backend.BufferData(buffer, getSoundFormat(_soundData.Channels, _soundData.BitsPerSample),
					shorts, _soundData.DataLength, _soundData.SampleRate);
			}
			else throw new NotSupportedException ("ALSound: Data type not supported: " + dataType.Name);

			_errors.HasErrors();
			return buffer;
		}

        private SoundFormat getSoundFormat(int channels, int bits)
		{
			switch (channels)

			{
				case 1: return bits == 8 ? SoundFormat.Mono8 : SoundFormat.Mono16;
				case 2: return bits == 8 ? SoundFormat.Stereo8 : SoundFormat.Stereo16;
				default: throw new NotSupportedException("The specified sound format is not supported.");
			}
		}
	}
}

