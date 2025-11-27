using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using System;

namespace AvaloniaOpenGLTest;

public class OpenGlTriangleControl : OpenGlControlBase
{
    private readonly float[] vertices = {
    -0.5f, -0.5f, 0.0f,  // bottom left
     0.5f, -0.5f, 0.0f,  // bottom right
     0.0f,  0.5f, 0.0f   // top
     };
    private const string VertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
void main()
{
    gl_Position = vec4(aPosition, 1.0);
}";

    private const string FragmentShaderSource = @"
#version 330 core
out vec4 FragColor;
void main()
{
    FragColor = vec4(1.0, 0.5, 0.2, 1.0);
}";
    private int vbo;
    private int vao;

    private int shaderProgram;

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        gl.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);  // dark blue background
        gl.Clear(GlConsts.GL_COLOR_BUFFER_BIT);
        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);
        gl.DrawArrays(GlConsts.GL_TRIANGLES, 0, 3);

    }
    protected override void OnOpenGlInit(GlInterface gl)
    {
        vbo = gl.GenBuffer();
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, vbo);
        unsafe
        {
            fixed (float* ptr = vertices)
            {
                gl.BufferData(GlConsts.GL_ARRAY_BUFFER, new IntPtr(sizeof(float) * vertices.Length), new IntPtr(ptr), GlConsts.GL_STATIC_DRAW);
            }
        }
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.VertexAttribPointer(0, 3, GlConsts.GL_FLOAT, 0, 0, IntPtr.Zero);
        gl.EnableVertexAttribArray(0);
        int vertexShader = CompileShader(gl, GlConsts.GL_VERTEX_SHADER, VertexShaderSource);
        int fragmentShader = CompileShader(gl, GlConsts.GL_FRAGMENT_SHADER, FragmentShaderSource);

        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        gl.LinkProgram(shaderProgram);

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }
    private int CompileShader(GlInterface gl, int type, string source)
{
    int shader = gl.CreateShader(type);
    
    unsafe
    {
        // Convert string to byte array with null terminator
        byte[] sourceBytes = System.Text.Encoding.UTF8.GetBytes(source + '\0');
        
        fixed (byte* sourcePtr = sourceBytes)
        {
            IntPtr stringPtr = new IntPtr(sourcePtr);
            IntPtr stringsPtr = new IntPtr(&stringPtr);
            gl.ShaderSource(shader, 1, stringsPtr, IntPtr.Zero);
        }
    }
    
    gl.CompileShader(shader);
    
    // Check for errors (optional but helpful)
    unsafe
    {
        int logLength;
        byte* logBuffer = stackalloc byte[1024];
        gl.GetShaderInfoLog(shader, 1024, out logLength, logBuffer);
        if (logLength > 0)
        {
            string error = System.Text.Encoding.UTF8.GetString(logBuffer, logLength);
            if (!string.IsNullOrWhiteSpace(error))
                throw new Exception($"Error compiling shader: {error}");
        }
    }
    
    return shader;
}
}