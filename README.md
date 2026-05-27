<a id="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]

<br />
<div align="center">

<h3 align="center">Sound Manager Advance</h3>

  <p align="center">
    <em>Sound Handler built in top of Unity C# AudioSource usign Editor Tools + Scriptable Objects approach.</em>
    <br />
    <br />
    <a href="https://github.com/github_username/repo_name"><strong>Ver documentación »</strong></a>
    <br />
<br />
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Content</summary>
  <ol>
    <li>
      <a href="#about-the-project">About SoundManager</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Dependencies</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->

## About Sound Manager Advance

_Sound Handler built in top of Unity C# AudioSource usign Editor Tools + Scriptable Objects approach._

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Built With

- ![Unity]

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->

## Getting Started

Clone this repository

### Dependencies

This list is not mandatory. All dependencies are optional and aim to ease the developers experience.

- Naughty Attributes
- FastPlay
- ~~DoTween~~ _(Not included yet)_

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->

## Usage

1. Put your audio files (AudioClips/Audio Random Containers) in **Resources** folder called **Audio/**
2. Once there click **Tools** > **Generate audio enums**.

```c#
 public enum AudioID
 {
     None = 0,
     Click,
     MouseClick,
     MouseRelease,
     Rollover,
     Switch,
 }
```

3. Start project to populate **AudioScriptables** with _AudioClips_ loaded.
4. Call **PlaySound, PlaySoundAt, PlayFlatSoundAt** methods to play a sound.

```c#
  // Play 3D Sound (Volume/Pitch variation from AudioScriptable)
  AudioManager.Instance.PlaySoundAt(AudioID.Switch, transform.position);
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ROADMAP -->

## Roadmap

- [ ] Audio Random Container integration
- [ ] DOTween optional integration
- [ ] More settings

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/Game-Dojo/Sound-Manager-Advance.svg?style=for-the-badge
[contributors-url]: https://github.com/Game-Dojo/Sound-Manager-Advance/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Game-Dojo/Sound-Manager-Advance.svg?style=for-the-badge
[forks-url]: https://github.com/Game-Dojo/Sound-Manager-Advanc/network/members
[stars-shield]: https://img.shields.io/github/stars/Game-Dojo/Sound-Manager-Advance.svg?style=for-the-badge
[stars-url]: https://github.com/Game-Dojo/repo_name/stargazers
[Unity]: https://img.shields.io/badge/Unity_6.3-000000?style=for-the-badge&logo=unity&logoColor=white
