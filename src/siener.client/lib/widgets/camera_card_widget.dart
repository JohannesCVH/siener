import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:siener.client/models/camera.dart';
import 'package:siener.client/services/camera_service.dart';

class CameraCardWidget extends StatefulWidget {
  final Camera camera;
  final CameraService cameraService;

  const CameraCardWidget({
    super.key, 
    required this.camera,
    required this.cameraService,
  });
  
  @override
  State<CameraCardWidget> createState() => _CameraCardWidgetState();
}

class _CameraCardWidgetState extends State<CameraCardWidget> {
  Uint8List? _imageData;

  @override
  void initState() {
    super.initState();
    _loadThumbnail();
  }

  @override
  void didUpdateWidget(CameraCardWidget oldWidget) {
    super.didUpdateWidget(oldWidget);
    _loadThumbnail();
  }

  Future<void> _loadThumbnail() async {
    _imageData = null;
    
    final bytes = await widget.cameraService.getThumbnailBytes(widget.camera.name);
    if (bytes != null && mounted) {
      setState(() {
        _imageData = bytes;
      });
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 4,
      clipBehavior: Clip.antiAlias,
      child: Column(
        children: [
          AspectRatio(
            aspectRatio: 16/9,
            child: _imageData != null
            ? Image.memory(_imageData!, fit: BoxFit.fitWidth, )
            : const Center(child: CircularProgressIndicator())
          ),
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: Text(
              widget.camera.name,
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
        ]
      )
    );
  }
}