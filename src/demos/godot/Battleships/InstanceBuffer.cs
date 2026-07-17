using Godot;

namespace fennecs.demos.godot.Battleships;

/// <summary>
/// A growable batch of 2D sprite instances rendered through a single
/// MultiMeshInstance2D — one draw call no matter how many shells or
/// explosions are in flight, and zero scene tree churn.
/// Instance data goes straight into the RenderingServer buffer:
/// 8 floats Transform2D, 4 floats color, 4 floats custom data
/// (custom.x carries animation progress for the frame-strip shader).
/// </summary>
internal sealed class InstanceBuffer
{
	private const int Stride = 16; // Transform2D (8) + Color (4) + Custom (4)

	private readonly MultiMesh _multiMesh;
	private float[] _buffer;
	private int _capacity;
	private int _count;

	public InstanceBuffer(Node parent, string name, Texture2D? texture, Shader shader, int hframes, int zIndex, int capacity = 512)
	{
		_capacity = capacity;
		_buffer = new float[_capacity * Stride];

		var material = new ShaderMaterial { Shader = shader };
		if (hframes > 1) material.SetShaderParameter("hframes", (float) hframes);

		_multiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			UseColors = true,
			UseCustomData = true,
			Mesh = new QuadMesh { Size = Vector2.One },
			InstanceCount = _capacity,
			// Instances live in world space far from the node's origin;
			// an effectively infinite AABB keeps the batch from being culled.
			CustomAabb = new Aabb(new Vector3(-1e7f, -1e7f, -1f), new Vector3(2e7f, 2e7f, 2f)),
		};

		parent.AddChild(new MultiMeshInstance2D
		{
			Name = name,
			Multimesh = _multiMesh,
			Texture = texture,
			Material = material,
			ZIndex = zIndex,
		});
	}

	public void Begin() => _count = 0;

	public void Add(Vector2 position, float rotation, Vector2 scale, Color color, float progress = 0f)
	{
		if (_count == _capacity) Grow();

		var cos = Mathf.Cos(rotation);
		var sin = Mathf.Sin(rotation);

		var i = _count++ * Stride;
		_buffer[i + 0] = cos * scale.X;
		_buffer[i + 1] = -sin * scale.Y;
		_buffer[i + 2] = 0f;
		_buffer[i + 3] = position.X;
		_buffer[i + 4] = sin * scale.X;
		_buffer[i + 5] = cos * scale.Y;
		_buffer[i + 6] = 0f;
		_buffer[i + 7] = position.Y;
		_buffer[i + 8] = color.R;
		_buffer[i + 9] = color.G;
		_buffer[i + 10] = color.B;
		_buffer[i + 11] = color.A;
		_buffer[i + 12] = progress;
		// 13..15 remain zero
	}

	public void Commit()
	{
		RenderingServer.MultimeshSetBuffer(_multiMesh.GetRid(), _buffer);
		_multiMesh.VisibleInstanceCount = _count;
	}

	private void Grow()
	{
		_capacity *= 2;
		System.Array.Resize(ref _buffer, _capacity * Stride);
		_multiMesh.InstanceCount = _capacity;
	}
}
