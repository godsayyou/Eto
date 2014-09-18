using System;
using Eto.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace Eto.Forms
{
	/// <summary>
	/// Layout for controls in a table
	/// </summary>
	/// <remarks>
	/// This is similar to an html table, though each control will fill its entire cell.
	/// </remarks>
	[ContentProperty("Rows")]
	[Handler(typeof(TableLayout.IHandler))]
	public class TableLayout : Layout
	{
		new IHandler Handler { get { return (IHandler)base.Handler; } }

		static object rowsKey = new object();
		static object contentsKey = new object();
		Size dimensions;
		bool created;

		/// <summary>
		/// The default spacing for all tables
		/// </summary>
		[Obsolete("Use styles to set control defaults, e.g. Style.Add<TableLayout>(null, table => table.Spacing = new Size(5));")]
		public static Size DefaultSpacing = Size.Empty;

		/// <summary>
		/// The default padding for all tables
		/// </summary>
		[Obsolete("Use styles to set control defaults, e.g. Style.Add<TableLayout>(null, table => table.Padding = new Padding(5));")]
		public static Padding DefaultPadding = Padding.Empty;

		/// <summary>
		/// Gets an enumeration of controls that are directly contained by this container
		/// </summary>
		/// <value>The contained controls.</value>
		public override IEnumerable<Control> Controls
		{
			get { return Rows.SelectMany(r => r.Cells).Select(r => r.Control).Where(r => r != null); }
		}

		/// <summary>
		/// Gets the collection of rows in the table
		/// </summary>
		/// <value>The rows.</value>
		public Collection<TableRow> Rows
		{
			get { return Properties.Create<TableRowCollection>(rowsKey); }
			private set { Properties[rowsKey] = value; }
		}

		/// <summary>
		/// Gets the collection of controls contained in this container.
		/// </summary>
		/// <value>The contents.</value>
		[Obsolete("Use Rows instead to add rows hierarchically")]
		public Collection<Control> Contents
		{
			get { return Properties.Create<Collection<Control>>(contentsKey); }
		}

		/// <summary>
		/// Gets or sets the dimensions of the table
		/// </summary>
		/// <value>The dimensions of the table.</value>
		[Obsolete("Use the Rows property to add the number of rows/cells for the table, or Dimensions to get the dimensions of the table")]
		public Size CellSize
		{
			get { return dimensions; }
			set
			{
				SetCellSize(value, true);
			}
		}

		/// <summary>
		/// Gets the dimensions of the table in cells.
		/// </summary>
		/// <value>The dimensions of the table.</value>
		public Size Dimensions
		{
			get { return dimensions; }
		}

		/// <summary>
		/// Gets or sets the scaled columns in the table.
		/// </summary>
		/// <value>The scaled columns</value>
		[Obsolete("Use the ScaleWidth on TableCell instead")]
		[TypeConverter(typeof(Int32ArrayConverter))]
		public int[] ColumnScale
		{
			set
			{
				for (int col = 0; col < dimensions.Width; col++)
				{
					SetColumnScale(col, false);
				}
				foreach (var col in value)
				{
					SetColumnScale(col);
				}
			}
			get
			{
				var vals = new List<int>();
				for (int col = 0; col < dimensions.Width; col++)
				{
					if (GetColumnScale(col))
						vals.Add(col);
				}
				return vals.ToArray();
			}
		}

		/// <summary>
		/// Gets or sets the scaled rows in the table.
		/// </summary>
		/// <value>The scaled rows.</value>
		[TypeConverter(typeof(Int32ArrayConverter))]
		[Obsolete("Use the ScaleHeight on TableRow instead")]
		public int[] RowScale
		{
			set
			{
				for (int row = 0; row < dimensions.Height; row++)
				{
					SetRowScale(row, false);
				}
				foreach (var row in value)
				{
					SetRowScale(row);
				}
			}
			get
			{
				var vals = new List<int>();
				for (int row = 0; row < dimensions.Height; row++)
				{
					if (GetRowScale(row))
						vals.Add(row);
				}
				return vals.ToArray();
			}
		}

		#region Attached Properties

		static readonly EtoMemberIdentifier LocationProperty = new EtoMemberIdentifier(typeof(TableLayout), "Location");

		/// <summary>
		/// Gets the table location of the specified control.
		/// </summary>
		/// <returns>The location.</returns>
		/// <param name="control">Control.</param>
		[Obsolete("Use Rows instead to add rows hierarchically")]
		public static Point GetLocation(Control control)
		{
			return control.Properties.Get<Point>(LocationProperty);
		}

		/// <summary>
		/// Sets the table location of the specified control.
		/// </summary>
		/// <returns>The location.</returns>
		/// <param name="control">Control to set the location.</param>
		/// <param name="value">Location value</param>
		[Obsolete("Use Rows instead to add rows hierarchically")]
		public static void SetLocation(Control control, Point value)
		{
			control.Properties[LocationProperty] = value;
			var layout = control.Parent as TableLayout;
			if (layout != null)
				layout.Move(control, value);
		}

		static readonly EtoMemberIdentifier ColumnScaleProperty = new EtoMemberIdentifier(typeof(TableLayout), "ColumnScale");

		/// <summary>
		/// Gets the column scale for the specified control.
		/// </summary>
		/// <returns><c>true</c>, if column scale was gotten, <c>false</c> otherwise.</returns>
		/// <param name="control">Control.</param>
		[Obsolete("Use TableCell instead to set column scaling, or get directly using GetColumnScale(int)")]
		public static bool GetColumnScale(Control control)
		{
			return control.Properties.Get<bool>(ColumnScaleProperty);
		}

		/// <summary>
		/// Sets the column scale for the specified control.
		/// </summary>
		/// <param name="control">Control.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		[Obsolete("Use TableCell instead to set column scaling, or set directly using SetColumnScale(int)")]
		public static void SetColumnScale(Control control, bool value)
		{
			control.Properties[ColumnScaleProperty] = value;
		}

		static readonly EtoMemberIdentifier RowScaleProperty = new EtoMemberIdentifier(typeof(TableLayout), "RowScale");

		/// <summary>
		/// Gets the row scale for the specified control.
		/// </summary>
		/// <returns><c>true</c>, if row scale was gotten, <c>false</c> otherwise.</returns>
		/// <param name="control">Control.</param>
		[Obsolete("Use TableRow instead to set row scaling, or get directly using GetRowScale(int)")]
		public static bool GetRowScale(Control control)
		{
			return control.Properties.Get<bool>(RowScaleProperty);
		}

		/// <summary>
		/// Sets the row scale for the specified control.
		/// </summary>
		/// <param name="control">Control.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		[Obsolete("Use TableRow instead to set row scaling, or get directly using SetRowScale(int)")]
		public static void SetRowScale(Control control, bool value)
		{
			control.Properties[RowScaleProperty] = value;
		}

		#endregion

		/// <summary>
		/// Creates a table layout with an auto sized control.
		/// </summary>
		/// <remarks>
		/// Since controls fill an entire cell, you can use this method to create a layout that will ensure that the
		/// specified <paramref name="control"/> gets its preferred size instead of stretching to fill the container.
		/// 
		/// By default, extra space will be added to the right and bottom, unless <paramref name="centered"/> is <c>true</c>,
		/// which will add equal space to the top/bottom, and left/right.
		/// </remarks>
		/// <returns>The table layout with the auto sized control.</returns>
		/// <param name="control">Control to auto size.</param>
		/// <param name="padding">Padding around the control</param>
		/// <param name="centered">If set to <c>true</c> center the control, otherwise control is upper left of the container.</param>
		public static TableLayout AutoSized(Control control, Padding? padding = null, bool centered = false)
		{
			var layout = new TableLayout(3, 3);
			layout.Padding = padding ?? Padding.Empty;
			layout.Spacing = Size.Empty;
			if (centered)
			{
				layout.SetColumnScale(0);
				layout.SetColumnScale(2);
				layout.SetRowScale(0);
				layout.SetRowScale(2);
			}
			layout.Add(control, 1, 1);
			return layout;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.TableLayout"/> class.
		/// </summary>
		public TableLayout()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.TableLayout"/> class with the specified number of columns and rows.
		/// </summary>
		/// <param name="columns">Number of columns in the table.</param>
		/// <param name="rows">Number of rows in the table.</param>
		public TableLayout(int columns, int rows)
			: this(new Size(columns, rows))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.TableLayout"/> class with the specified dimensions.
		/// </summary>
		/// <param name="dimensions">Dimensions of the table.</param>
		public TableLayout(Size dimensions)
		{
			SetCellSize(dimensions, true);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.TableLayout"/> class with the specified rows.
		/// </summary>
		/// <param name="rows">Rows to populate the table.</param>
		public TableLayout(params TableRow[] rows)
		{
			Rows = new TableRowCollection(rows);
			Create();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.TableLayout"/> class with the specified rows.
		/// </summary>
		/// <param name="rows">Rows to populate the table.</param>
		public TableLayout(IEnumerable<TableRow> rows)
		{
			Rows = new TableRowCollection(rows);
			Create();
		}

		void Create()
		{
			var rows = Rows;
			var columnCount = rows.Max(r => r != null ? r.Cells.Count : 0);
			SetCellSize(new Size(columnCount, rows.Count), false);
			for (int y = 0; y < rows.Count; y++)
			{
				var row = rows[y];
				while (row.Cells.Count < columnCount)
					row.Cells.Add(new TableCell());
				for (int x = 0; x < columnCount; x++)
				{
					var cell = row.Cells[x];
					Add(cell.Control, x, y);
					if (cell.ScaleWidth)
						SetColumnScale(x);
				}
				while (row.Cells.Count < columnCount)
					row.Cells.Add(new TableCell());
				if (row.ScaleHeight)
					SetRowScale(y);
			}
			created = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.TableLayout"/> class.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="generator">Generator.</param>
		[Obsolete("Use constructor without generator instead")]
		public TableLayout(int width, int height, Generator generator = null)
			: this(new Size(width, height), generator)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.TableLayout"/> class.
		/// </summary>
		/// <param name="size">Size.</param>
		/// <param name="generator">Generator.</param>
		[Obsolete("Use constructor without generator instead")]
		public TableLayout(Size size, Generator generator = null)
			: base(generator, typeof(IHandler), false)
		{
			SetCellSize(size, true);
		}

		void SetCellSize(Size value, bool createRows)
		{
			if (created)
				throw new InvalidOperationException("Can only set the cell size of a table once");
			dimensions = value;
			if (!dimensions.IsEmpty)
			{
				Handler.CreateControl(dimensions.Width, dimensions.Height);
				Initialize();
				if (createRows)
				{
					var rows = Enumerable.Range(0, value.Height).Select(r => new TableRow(Enumerable.Range(0, value.Width).Select(c => new TableCell())));
					Rows = new TableRowCollection(rows);
					created = true;
				}
			}
		}

		/// <summary>
		/// Sets the scale for the specified column.
		/// </summary>
		/// <param name="column">Column to set the scale for.</param>
		/// <param name="scale">If set to <c>true</c> scale, otherwise size to preferred size of controls in the column.</param>
		public void SetColumnScale(int column, bool scale = true)
		{
			Handler.SetColumnScale(column, scale);
		}

		/// <summary>
		/// Gets the scale for the specified column.
		/// </summary>
		/// <returns><c>true</c>, if column is scaled, <c>false</c> otherwise.</returns>
		/// <param name="column">Column to retrieve the scale.</param>
		public bool GetColumnScale(int column)
		{
			return Handler.GetColumnScale(column);
		}

		/// <summary>
		/// Sets the scale for the specified row.
		/// </summary>
		/// <param name="row">Row to set the scale for.</param>
		/// <param name="scale">If set to <c>true</c> scale, otherwise size to preferred size of controls in the row.</param>
		public void SetRowScale(int row, bool scale = true)
		{
			Handler.SetRowScale(row, scale);
		}

		/// <summary>
		/// Gets the scale for the specified row.
		/// </summary>
		/// <returns><c>true</c>, if row is scaled, <c>false</c> otherwise.</returns>
		/// <param name="row">Row to retrieve the scale.</param>
		public bool GetRowScale(int row)
		{
			return Handler.GetRowScale(row);
		}

		/// <summary>
		/// Adds a control to the specified x &amp; y coordinates.
		/// </summary>
		/// <remarks>
		/// If a control already exists in the location, it is replaced. Only one control can exist in a cell.
		/// </remarks>
		/// <param name="control">Control to add.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void Add(Control control, int x, int y)
		{
			if (control != null)
				control.Properties[LocationProperty] = new Point(x, y);
			InnerAdd(control, x, y);
		}

		void InnerAdd(Control control, int x, int y)
		{
			if (dimensions.IsEmpty)
				throw new InvalidOperationException("You must set the size of the TableLayout before adding controls");
			var cell = Rows[y].Cells[x];

			SetParent(control, () => {
				cell.Control = control;
				Handler.Add(control, x, y);
			}, cell.Control);
		}

		/// <summary>
		/// Adds a control to the specified x &amp; y coordinates.
		/// </summary>
		/// <remarks>
		/// If a control already exists in the location, it is replaced. Only one control can exist in a cell.
		/// The <paramref name="xscale"/> and <paramref name="yscale"/> parameters are to easily set the scaling
		/// for the current row/column while adding the control.
		/// </remarks>
		/// <param name="control">Control to add.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="xscale">If set to <c>true</c> xscale.</param>
		/// <param name="yscale">If set to <c>true</c> yscale.</param>
		public void Add(Control control, int x, int y, bool xscale, bool yscale)
		{
			control.Properties[LocationProperty] = new Point(x, y);
			SetColumnScale(x, xscale);
			SetRowScale(y, yscale);
			Add(control, x, y);
		}

		/// <summary>
		/// Adds a control to the specified location.
		/// </summary>
		/// <remarks>
		/// If a control already exists in the location, it is replaced. Only one control can exist in a cell.
		/// </remarks>
		/// <param name="control">Control to add.</param>
		/// <param name="location">The location of the control.</param>
		public void Add(Control control, Point location)
		{
			Add(control, location.X, location.Y);
		}

		/// <summary>
		/// Moves the specified control to the new x and y coordinates.
		/// </summary>
		/// <remarks>
		/// If a control already exists in the new location, it will be replaced. Only one control can exist in a cell.
		/// The old location of the control will have an empty space.
		/// </remarks>
		/// <param name="control">Control to move.</param>
		/// <param name="x">The new x coordinate.</param>
		/// <param name="y">The new y coordinate.</param>
		public void Move(Control control, int x, int y)
		{
			var cell = Rows.SelectMany(r => r.Cells).FirstOrDefault(r => r.Control == control);
			if (cell != null)
				cell.Control = null;

			cell = Rows[y].Cells[x];
			var old = cell.Control;
			if (old != null)
				RemoveParent(old);
			cell.Control = control;
			control.Properties[LocationProperty] = new Point(x, y);
			Handler.Move(control, x, y);
		}

		/// <summary>
		/// Move the specified control to a new location.
		/// </summary>
		/// <remarks>
		/// If a control already exists in the new location, it will be replaced. Only one control can exist in a cell.
		/// The old location of the control will have an empty space.
		/// </remarks>
		/// <param name="control">Control to move.</param>
		/// <param name="location">New location of the control.</param>
		public void Move(Control control, Point location)
		{
			Move(control, location.X, location.Y);
		}

		/// <summary>
		/// Remove the specified child control.
		/// </summary>
		/// <param name="child">Child control to remove.</param>
		public override void Remove(Control child)
		{
			var cell = Rows.SelectMany(r => r.Cells).FirstOrDefault(r => r.Control == child);
			if (cell != null)
			{
				cell.Control = null;
				Handler.Remove(child);
				RemoveParent(child);
			}
		}

		/// <summary>
		/// Gets or sets the horizontal and vertical spacing between each of the cells of the table.
		/// </summary>
		/// <value>The spacing between the cells.</value>
		public Size Spacing
		{
			get { return Handler.Spacing; }
			set { Handler.Spacing = value; }
		}

		/// <summary>
		/// Gets or sets the padding bordering the table.
		/// </summary>
		/// <value>The padding bordering the table.</value>
		public Padding Padding
		{
			get { return Handler.Padding; }
			set { Handler.Padding = value; }
		}

		[OnDeserialized]
		void OnDeserialized(StreamingContext context)
		{
			OnDeserialized();
		}

		/// <summary>
		/// Ends the initialization when loading from xaml or other code generated scenarios
		/// </summary>
		public override void EndInit()
		{
			base.EndInit();
			OnDeserialized(Parent != null); // mono calls EndInit BEFORE setting to parent
		}

		/// <summary>
		/// Raises the <see cref="Control.PreLoad"/> event, and recurses to this container's children
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnPreLoad(EventArgs e)
		{
			OnDeserialized(true);
			base.OnPreLoad(e);
		}

		void OnDeserialized(bool direct = false)
		{
			if (Loaded || direct)
			{
				if (!created)
				{
					var rows = Properties.Get<Collection<TableRow>>(rowsKey);
					if (rows != null)
					{
						Create();
					}
				}

				#pragma warning disable 612,618
				// remove when obsolete code is removed
				var contents = Properties.Get<Collection<Control>>(contentsKey);
				if (contents != null)
				{
					foreach (var control in contents)
					{
						var location = GetLocation(control);
						Add(control, location);
						if (GetColumnScale(control))
							SetColumnScale(location.X);
						if (GetRowScale(control))
							SetRowScale(location.Y);
					}
					Properties.Remove(contentsKey);
				}
				#pragma warning restore 612,618
			}
		}

		/// <summary>
		/// Handler interface for <see cref="TableLayout"/>
		/// </summary>
		/// <remarks>
		/// Currently, TableLayout handlers only need to set its size while created and cannot be resized.
		/// </remarks>
		[AutoInitialize(false)]
		public new interface IHandler : Layout.IHandler, IPositionalLayoutHandler
		{
			/// <summary>
			/// Creates the control with the specified dimensions.
			/// </summary>
			/// <param name="columns">Number of columns for the table.</param>
			/// <param name="rows">Number of rows for the table.</param>
			void CreateControl(int columns, int rows);

			/// <summary>
			/// Gets the scale for the specified column.
			/// </summary>
			/// <returns><c>true</c>, if column is scaled, <c>false</c> otherwise.</returns>
			/// <param name="column">Column to retrieve the scale.</param>
			bool GetColumnScale(int column);

			/// <summary>
			/// Sets the scale for the specified column.
			/// </summary>
			/// <param name="column">Column to set the scale for.</param>
			/// <param name="scale">If set to <c>true</c> scale, otherwise size to preferred size of controls in the column.</param>
			void SetColumnScale(int column, bool scale);

			/// <summary>
			/// Gets the scale for the specified row.
			/// </summary>
			/// <returns><c>true</c>, if row is scaled, <c>false</c> otherwise.</returns>
			/// <param name="row">Row to retrieve the scale.</param>
			bool GetRowScale(int row);

			/// <summary>
			/// Sets the scale for the specified row.
			/// </summary>
			/// <param name="row">Row to set the scale for.</param>
			/// <param name="scale">If set to <c>true</c> scale, otherwise size to preferred size of controls in the row.</param>
			void SetRowScale(int row, bool scale);

			/// <summary>
			/// Gets or sets the horizontal and vertical spacing between each of the cells of the table.
			/// </summary>
			/// <value>The spacing between the cells.</value>
			Size Spacing { get; set; }

			/// <summary>
			/// Gets or sets the padding bordering the table.
			/// </summary>
			/// <value>The padding bordering the table.</value>
			Padding Padding { get; set; }
		}

		/// <summary>
		/// Implicitly converts an array of rows to a vertical TableLayout
		/// </summary>
		/// <param name="rows">Rows to convert.</param>
		public static implicit operator TableLayout(TableRow[] rows)
		{
			return new TableLayout(rows);
		}
	}
}
