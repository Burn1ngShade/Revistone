using System.Diagnostics.Contracts;
using System.IO.IsolatedStorage;

namespace Revistone
{
    namespace Console
    {
        namespace Image
        {
            /// <summary> Class pertaining all logic for creating videos in the console. </summary>
            public class ConsoleVideo
            {
                // --- VARIABLES AND CONSTRUCTORS ---

                List<ConsoleImage> _frames;
                public ConsoleImage[] frames { get { return _frames.ToArray(); } }

                int _ticksPerFrame;
                public int ticksPerFrame { get { return _ticksPerFrame; } }

                bool _paused = false;
                public bool paused { get { return _paused; } }

                /// <summary> Class pertaining all logic for creating videos in the console. </summary>
                public ConsoleVideo(ConsoleImage[] frames, int ticksPerFrame = 10)
                {
                    _frames = frames.ToList();
                    _ticksPerFrame = Math.Clamp(ticksPerFrame, 1, int.MaxValue);
                }

                /// <summary> Class pertaining all logic for creating videos in the console. </summary>
                public ConsoleVideo() : this(new ConsoleImage[] { }, 10) { }

                // --- FRAME CONTROL ---

                /// <summary> Updates the frame rate of video. </summary>
                public void UpdateFrameRate(int ticksPerFrame = 10)
                {
                    _ticksPerFrame = Math.Clamp(ticksPerFrame, 1, int.MaxValue);
                }

                /// <summary> Adds frame to video at given frame index. </summary>
                public void AddFrame(ConsoleImage frame, int frameIndex) { _frames.Insert(frameIndex, frame); }

                /// <summary> Adds frame to end of the video. </summary>
                public void AddFrame(ConsoleImage frame) { _frames.Add(frame); }

                // --- OUTPUT ---

                public void SetPauseState(bool pause = true)
                {
                    _paused = pause;
                }

                /// <summary> Outputs video to console. </summary>
                public bool SendToConsole(bool looping = false, bool colourless = false)
                {
                    if (_frames.Count == 0) return false;
                    ConsoleLine[] c = _frames[0].ToConsoleLineArray(colourless);

                    for (int i = 0; i < c.Length; i++)
                    {
                        ConsoleAction.SendConsoleMessage(c[i], new ConsoleAnimatedLine(UpdateImage, (this, 0, i, new bool[] { looping, colourless }), ticksPerFrame, true));
                    }

                    return true;
                }

                /// <summary> Updates video, used by dynamic line. </summary>
                void UpdateImage(ConsoleLine consoleLine, ConsoleAnimatedLine animationInfo, int tickNum)
                {
                    if (paused) return;

                    if (animationInfo.metaInfo as (ConsoleVideo, int, int, bool[])? == null) return;

                    (ConsoleVideo video, int frameIndex, int lineIndex, bool[] modifications) info = ((ConsoleVideo, int, int, bool[]))animationInfo.metaInfo;

                    consoleLine.Update(info.video.frames[info.frameIndex].RowToConsoleLine(info.video.frames[info.frameIndex].size.height - 1 - info.lineIndex, info.modifications[1]));

                    if (info.video.frames.Length - 1 <= info.frameIndex)
                    {
                        if (info.modifications[0])
                        {
                            info.frameIndex = 0;
                            animationInfo.metaInfo = info;
                        }
                        else
                        {
                            animationInfo.enabled = false;
                            return;
                        }

                    }
                    else
                    {
                        info.frameIndex++;
                        animationInfo.metaInfo = info;
                    }
                }
            }
        }
    }
}