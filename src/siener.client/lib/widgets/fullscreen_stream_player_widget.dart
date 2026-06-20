import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_webrtc/flutter_webrtc.dart';

class FullScreenStreamPlayerWidget extends StatefulWidget {
  final RTCVideoRenderer renderer;

  const FullScreenStreamPlayerWidget({super.key, required this.renderer});

  @override
  State<FullScreenStreamPlayerWidget> createState() => _FullScreenStreamPlayerWidgetState();
}

class _FullScreenStreamPlayerWidgetState extends State<FullScreenStreamPlayerWidget> {
  @override
  void initState() {
    super.initState();
    // Force Landscape orientation (equivalent to Orientation configuration in Xamarin/MAUI)
    SystemChrome.setPreferredOrientations([
      DeviceOrientation.landscapeLeft,
      DeviceOrientation.landscapeRight,
    ]);
    // Hide Status bar and Bottom Navigation Bar (Immersive Full Screen)
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersiveSticky);
  }

  @override
  void dispose() {
    // Restore orientation back to normal portrait when exiting full screen
    SystemChrome.setPreferredOrientations([
      DeviceOrientation.portraitUp,
    ]);
    // Restore standard device notification/navigation overlays
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      body: Stack(
        children: [
          // Center the 16:9 stream on screen
          Center(
            child: AspectRatio(
              aspectRatio: 16 / 9,
              child: RTCVideoView(
                widget.renderer,
                objectFit: RTCVideoViewObjectFit.RTCVideoViewObjectFitContain,
              ),
            ),
          ),
          
          // Back/Exit button in top-left
          Positioned(
            top: 16,
            right: 48,
            child: SafeArea(
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.black.withOpacity(0.5),
                  shape: BoxShape.circle,
                ),
                child: IconButton(
                  icon: const Icon(Icons.fullscreen_exit, color: Colors.white, size: 28),
                  onPressed: () => Navigator.of(context).pop(),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}