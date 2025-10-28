# MonoGame Headless Graphics Testing - Cross-Platform CI

## Overview

Testing graphics code in CI environments is a pain.  Your tests need a `GraphicsDevice`, but CI runners don't have displays attached.  This project demonstrates how to actually test MonoGame graphics functionality in GitHub Actions for Windows and Linux.

The short version: [WARP](https://learn.microsoft.com/en-us/windows/win32/direct3darticles/directx-warp) on Windows, [XVFB](https://www.x.org/archive/X11R7.7/doc/man/man1/Xvfb.1.xhtml) on Linux.

## The Problem

If you have tried to run MonoGame tests in a headless CI environment, you have probably seen this:

```sh
Failed to Initialize GraphicsDevice
```

Your tests work fine locally because you have a GPU and a display.  CI runners, however, are headless; no display and often no GPU drivers.  MonoGame needs a graphics device to do basically anything graphics related, so your tests just...fail.

## How this Works

Each platform has its own solution:

### Windows

Windows uses [WARP (Windows Advanced Rasterization Platform)](https://learn.microsoft.com/en-us/windows/win32/direct3darticles/directx-warp).  It is built into Windows, so you don't need to install anything.  We use `MonoGame.Framework.WindowsDX` and the fixture automatically picks WARP when running in a CI.

### Linux

Linux uses [XVFB (X Virtual Framebuffer)](https://www.x.org/archive/X11R7.7/doc/man/man1/Xvfb.1.xhtml), which creates a fake X11 display.  You need to install and wrap your test command with `xvfb-run`.  We use `MonoGame.Framework.DesktopGL` here.

### macOS

Currently I cannot find a reliable solution for macOS.  macOS does have built-in headless OpenGL support, however, in a headless environment, it is unable to create an OpenGL context, so the tests fail.  So for now, no macOS solution.

## Project Structure

```sh
MonoGameHeadlessGraphicsTesting/
├── Fixtures/
│   ├── GraphicsTestFixture.cs       # Sets up the Game and GraphicsDevice
│   └── GraphicsTestCollection.cs    # Prevents SDL from exploding
├── GraphicsDeviceTests.cs           # Basic graphics device operations
└── RenderingTests.cs                # Actual rendering operations

.github/workflows/
└── test.yml                         # CI setup for all platforms
```

## The Important Parts

### Cross-Platform Package References

The `.csproj` file uses MSBuild conditions to pick the right MonoGame package:

```xml
<IsWindows>$([MSBuild]::IsOSPlatform('Windows'))</IsWindows>
<IsLinux>$([MSBuild]::IsOSPlatform('Linux'))</IsLinux>
<IsOSX>$([MSBuild]::IsOSPlatform('OSX'))</IsOSX>

<!-- Windows gets DirectX -->
<ItemGroup Condition="$(IsWindows)">
  <PackageReference Include="MonoGame.Framework.WindowsDX" Version="3.8.4.1" />
</ItemGroup>

<!-- Linux and macOS get OpenGL -->
<ItemGroup Condition="$(IsLinux) OR $(IsOSX)">
  <PackageReference Include="MonoGame.Framework.DesktopGL" Version="3.8.4.1" />
</ItemGroup>
```

This means your tests automatically use the right MonoGame package depending on which operating system they are running on.

### GraphicsTestFixture

This is the core of the testing setup. It creates a MonoGame `Game` instance, initializes the graphics device, and gives you a read-to-use `SpriteBatch`.  On Windows, it explicitly looks for the WARP adapter to ensure software rendering works in the CI environment.

It also includes helper methods like `CreateTestTexture()` for quickly setting up scenarios.

### GraphicsTestCollection

This is a gotcha that I rand into.  Originally I was using `IClassFixture<T>` which works fine for a single test class  If multiple test classes each create their own `Game` instance though, SDL tries to initialize multiple times simultaneously and crashes with a `0xC0000005` error.

The solution for this was to instead use xUnit's collection fixtures.  This is done by having the `GraphicsTestCollection` file and then using `[Collection("GraphicsTest")]` on all test classes so they share a single fixture instance.  This way SDL only initializes once.

```cs
[Collection("GraphicsTest")]
public class GraphicsDeviceTests
{
    private readonly GraphicsTestFixture _graphicsFixture;
    // ...
}
```

## What Gets Tested

For demonstration purposes, the tests created here will test the following

- `GraphicsDeviceTests.cs` covers the basics:
  - Creating render targets
  - Setting graphics states (blend modes, depth/stencil, rasterizer)
  - Viewport operations
  - Creating vertex and index buffers
- `RenderingTests.cs` tests actual rendering:
  - Drawing with `SpriteBatch`
  - Rendering to render targets and reading data back
  - Different blend modes and sampler states
  - Multiple render targets
  - Various texture sizes

## Running the Tests

If you are running these tests locally, you just need to use `dotnet test`.  It'll work if you have a display.

If you are running these in a CI (e.g. GitHub Actions), then you'll need to install xvfb and run the tests using it the Linux

```yml
# Linux needs xvfb
- name: Setup xvfb
  run: |
    sudo apt-get update
    sudo apt-get install -y xvfb

# Linux runs tests with xvfb
- name: Run tests
  run: |
    xvfb-run -a -s "-screen 0 1024x768x24" dotnet test
```

For windows, the tests `GraphicsTestFixture` will automatically select the WARP adapter, so nothing special needs to be done in the testing workflow file.

```yml
# Windows just test as usual
- name: Run tests
  run: dotnet test
```

## Things to Know

**Software rendering is slow**.  WARP and software OpenGL are not going to give you GPU-level performance.  this is fine for unit testing, but not for benchmarking.

**WARP is reliable**.  Despite being software rendering, WARP is robust and well-tested by Microsoft.  It is used internally for various Windows components.

**xvfb can be finicky**.  Sometimes you need to tweak the screen size or color depth.  The `-a` flag (auto-select display) is important to avoid conflicts.

**Do not use this for performance testing**.  These tests verify correctness, not speed.  If you need to measure frame times or rendering performance, you'll need actual hardware.

## License

The code here is released to the public domain under the UNLICENSE.  See [LICENSE](LICENSE) for full text.
