import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'package:tes/firebase_options.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/app.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );

  IoCContainer.setup();
  runApp(App());
}
