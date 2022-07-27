using PerfectlyNormalUnity.DebugLogger_Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PerfectlyNormalUnity
{
    /// <summary>
    /// This class is used to log visuals and text, save to a file, then view from a c# app
    /// </summary>
    /// <remarks>
    /// All colors are hex format.  ARGB, alpha is optional
    ///     "444" becomes "FF444444" (very dark gray)
    ///     "806A" becomes "880066AA" (dusty blue)
    ///     "FF8040" becomes "FFFF8040" (sort of a coral)
    ///     "80FFFFFF" (50% transparent white)
    ///     
    /// Copied from:
    /// https://github.com/charlierix/CyberpunkMods/blob/main/grappling_hook/cet/core/debug_render_logger.lua
    /// https://github.com/charlierix/CyberpunkMods/blob/main/wall_hang/core/debug_render_logger.lua
    /// https://github.com/charlierix/CyberpunkMods/blob/main/debug_render_log/cet/code/debug_render_logger.lua
    ///
    /// Viewed with:
    /// https://github.com/charlierix/PartyPeople/tree/master/Math_WPF/WPF/DebugLogViewer
    /// https://github.com/charlierix/CyberpunkMods/tree/main/debug_render_log/DebugRenderViewer
    /// </remarks>
    public class DebugLogger
    {
        #region Declaration Section

        private readonly bool _enable_logging;

        private readonly string _folder;

        private readonly List<Category> _categories = new List<Category>();
        private readonly List<LogFrame> _frames = new List<LogFrame>();
        private readonly List<Text> _global_text = new List<Text>();

        #endregion

        #region Constructor

        public DebugLogger(string folder, bool enable_logging)
        {
            _folder = folder;
            _enable_logging = enable_logging;

            NewFrame();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This can make it easier to group similar items into the same category.  All items will be shown with
        /// the color and size specified here
        /// </summary>
        public void DefineCategory(string name = null, Color? color = null, float? size_mult = null)
        {
            _categories.Add(new Category()
            {
                name = name,
                color = color,
                size_mult = size_mult,
            });
        }

        /// <summary>
        /// Allows for multiple sets of points to be added.  The viwer will show one frame at a time
        /// </summary>
        /// <remarks>
        /// This is optional.When used, all add actions get tied to the current frame.  This allows views to
        /// be logged over time, then the viewer will have a scrollbar to flip between frames
        /// 
        /// There is no need to call this after instantiation, a first frame is implied (but there's no harm in
        /// explicitly calling it for the first frame)
        /// </remarks>
        public void NewFrame(string name = null, Color? back_color = null)
        {
            if (!_enable_logging)
                return;

            _frames.Add(new LogFrame()
            {
                name = name,
                back_color = back_color,
                items = new ItemBase[0],
                text = new Text[0],
            });
        }

        // Category and after are all optional.If a category is passed in, then the item will use that
        // category's size and color, unless overridden in the add method call
        public void Add_Dot(Vector3 position, Category category = null, Color? color = null, double? size_mult = null, string tooltip = null)
        {
            if (!_enable_logging)
                return;

            UtilityCore.ArrayAdd(_frames[_frames.Count - 1].items, new ItemDot()
            {
                category = category,
                color = color,
                size_mult = size_mult,
                tooltip = tooltip,

                position = position,
            });
        }
        public void Add_Line(Vector3 point1, Vector3 point2, Category category = null, Color? color = null, double? size_mult = null, string tooltip = null)
        {
            if (!_enable_logging)
                return;

            UtilityCore.ArrayAdd(_frames[_frames.Count - 1].items, new ItemLine()
            {
                category = category,
                color = color,
                size_mult = size_mult,
                tooltip = tooltip,

                point1 = point1,
                point2 = point2,

            });
        }
        public void Add_Circle(Vector3 center, Vector3 normal, float radius, Category category = null, Color? color = null, double? size_mult = null, string tooltip = null)
        {
            if (!_enable_logging)
                return;

            UtilityCore.ArrayAdd(_frames[_frames.Count - 1].items, new ItemCircle_Edge()
            {
                category = category,
                color = color,
                size_mult = size_mult,
                tooltip = tooltip,

                center = center,
                normal = normal,
                radius = radius,
            });
        }
        public void Add_Square(Vector3 center, Vector3 normal, float size_x, float size_y, Category category = null, Color? color = null, double? size_mult = null, string tooltip = null)
        {
            if (!_enable_logging)
                return;

            UtilityCore.ArrayAdd(_frames[_frames.Count - 1].items, new ItemSquare_Filled()
            {
                category = category,
                color = color,
                size_mult = size_mult,
                tooltip = tooltip,

                center = center,
                normal = normal,
                size_x = size_x,
                size_y = size_y,
            });
        }

        /// <summary>
        /// This is any extra text attached to the frame, useful for including log dumps next to the picture
        /// </summary>
        public void WriteLine_Frame(string text, Color? color, float? fontsize_mult)
        {
            if (!_enable_logging)
                return;

            UtilityCore.ArrayAdd(_frames[_frames.Count - 1].text, new Text()
            {
                text = text,
                color = color,
                fontsize_mult = fontsize_mult,
            });
        }
        /// <summary>
        /// This is text that is shown regardless of frames
        /// </summary>
        public void WriteLine_Global(string text, Color? color, float? fontsize_mult)
        {
            if (!_enable_logging)
                return;

            _global_text.Add(new Text()
            {
                text = text,
                color = color,
                fontsize_mult = fontsize_mult,
            });
        }

        /// <summary>
        /// This returns true if there is something to save (this ignores categories, since that is considered
        /// prep, and not real logged items)
        /// </summary>
        public bool IsPopulated()
        {
            if (!_enable_logging)
                return false;

            if (_global_text.Count > 0)
                return true;

            return _frames.Any(o => o.items.Length > 0 || o.text.Length > 0);
        }

        /// <summary>
        /// Saves everything into a file, clears all frames, so more can be added.  Keeps categories
        /// </summary>
        /// <param name="name"></param>
        public void Save(string name = null)
        {
            if (!_enable_logging)
                return;

            PossiblyRemoveFirstFrame();

            var scene = new LogScene()
            {
                categories = _categories.ToArray(),
                frames = _frames.ToArray(),
                text = _global_text.ToArray(),
            };

            string filename = GetFilename(name);

            File.WriteAllText(filename, JsonUtility.ToJson(scene, true));

            _frames.Clear();
            _global_text.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The constructor calls NewFrame with no params.  But the user might immediately call NewFrame with params.  That
        /// would leave an empty first frame.This function detects and removes that first frame
        /// </summary>
        /// <param name=""></param>
        private void PossiblyRemoveFirstFrame()
        {
            if (_frames.Count < 2)
                return;

            if (string.IsNullOrWhiteSpace(_frames[0].name) && _frames[0].back_color == null && _frames[0].items.Length == 0 && _frames[0].text.Length == 0)
                _frames.RemoveAt(0);
        }

        private string GetFilename(string name)
        {
            string retVal = Path.Combine(_folder, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            if (!string.IsNullOrWhiteSpace(name))
                retVal += " - " + name;

            retVal += ".json";

            return retVal;
        }

        #endregion
    }
}
