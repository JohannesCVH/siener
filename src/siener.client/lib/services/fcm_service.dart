import 'dart:convert';

import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:siener.client/http_client.dart';
import 'package:siener.client/logger.dart';

class FcmService {
  final String _baseUrl;
  final int _basePort;

  FcmService(): 
    _baseUrl = dotenv.get('API_BASE_URL'),
    _basePort = int.parse(dotenv.get('API_BASE_PORT'));

  Future<void> registerToken() async {
    try {
      String? token = await FirebaseMessaging.instance.getToken();
      if (token == null) {
        logError(FcmService, 'registerToken', 'FCM token is null.');
        return;
      }

      logMessage(FcmService, 'registerToken', 'Retrieved FCM Token: $token');

      final client = await getHttpClientWithCert();
      final response = await client.post(
        Uri.parse('$_baseUrl:$_basePort/api/Notification/RegisterToken'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'token': token}),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        logMessage(FcmService, 'registerToken', 'Successfully registered FCM token with API.');
      } else {
        logError(FcmService, 'registerToken', 'Failed to register token. Status: ${response.statusCode}');
      }
    } catch (e) {
      logError(FcmService, 'registerToken', 'Exception while registering FCM token: $e');
    }
  }
}