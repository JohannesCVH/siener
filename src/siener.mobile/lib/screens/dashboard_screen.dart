import 'package:flutter/material.dart';
import 'package:siener.mobile/logger.dart';
import 'package:siener.mobile/models/camera.dart';
import 'package:siener.mobile/screens/camera_screen.dart';
import 'package:siener.mobile/services/camera_service.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});
  
  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {

  final CameraService _cameraService = CameraService();
  List<Camera>? _cameras;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadCameras();
  }

  Future<void> _loadCameras() async {
    const String functionName = '_loadCameras';
    
    _isLoading = true;
    final List<Camera>? cameras = await _cameraService.fetchCameras();
    
    if (cameras != null) {
      setState(() {
        _cameras = cameras;
        _cameras?.forEach((camera) => logMessage(_DashboardScreenState, functionName, 'Camera loaded: ${camera.name}'));
        _isLoading = false;
      });
    } else {
      setState(() {
        _isLoading = false;
      });
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: ElevatedButton(
          onPressed: () {
            Navigator.push(
              context,
              MaterialPageRoute(builder: (context) => const CameraScreen()),
            );
          }, child: const Text('Camera'),
        ),
      ),
    );
  }
}