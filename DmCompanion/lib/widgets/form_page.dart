import 'package:flutter/material.dart';
import 'package:tes/widgets/page_template.dart';
import 'package:tes/widgets/rounded_window.dart';

class FormPage extends StatelessWidget {
  final String title;
  final List<Widget> children;

  const FormPage({super.key, required this.title, required this.children});

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
      title: title,
      child: Padding(
        padding: const EdgeInsets.all(40.0),
        child: Center(
          child: SizedBox(
            width: 600,
            child: RoundedWindow(
                child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: _buildFormContent(),
            )),
          ),
        ),
      ),
    );
  }

  Widget _buildFormContent() {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: List.generate(children.length, (index) {
          return Padding(
            padding:
                EdgeInsets.only(bottom: index < children.length - 1 ? 24.0 : 0),
            child: children[index],
          );
        }),
      ),
    );
  }
}
