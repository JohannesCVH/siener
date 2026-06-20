# Siener

NVR program that takes local camera RTSP streams and makes WebRTC streams which are viewable on the mobile client. In the future I want to add AI object detection & notifications.
NVR program written in C# (WebAPI) and Dart/Flutter (Mobile client).

## What it does

The project currently consists of 2 parts, siener.api and siener.client.

## siener.api

This is the backbone of the project:
* Spawns a FFMPEG process for each camera to read and save a frame every second.
* Spawns a MediaMTX process, each camera's RTSP stream then gets pushed to MediaMTX which creates WebRTC streams.

## siener.client

This is the mobile client where you can view different camera streams, (future) get notifications about detected objects like people/pets/cars and (future) manage settings.

## Roadmap
- [ ] Create a dashboard with navigation to different camera streams.
- [ ] Create a Login/Register page.
- [ ] Create a settings page to change camera notification settings.
- [ ] Implement AI object detection with notifications.

## License
This project is licensed under the GNU Affero General Public License v3.0 - see the [LICENSE](LICENSE) file for details.