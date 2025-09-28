import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/event.dart';
import 'package:tes/services/event_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/dialog_header.dart';
import 'package:tes/widgets/styled_dialog.dart';

class AddEventDialog extends StatefulWidget {
  final Campaign campaign;

  AddEventDialog({super.key, required this.campaign});

  @override
  State<AddEventDialog> createState() => _AddEventDialogState();
}

class _AddEventDialogState extends State<AddEventDialog> {
  final _eventService = get<EventService>();

  final _nameController = TextEditingController();
  final _locationController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _yearController = TextEditingController();
  final _monthController = TextEditingController();
  final _dayController = TextEditingController();

  void _updateState() {
    setState(() {});
  }

  @override
  void initState() {
    super.initState();
    _nameController.addListener(_updateState);
  }

  @override
  void dispose() {
    _nameController.removeListener(_updateState);
    _nameController.dispose();
    _locationController.dispose();
    _descriptionController.dispose();
    _yearController.dispose();
    _monthController.dispose();
    _dayController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return StyledDialog(
      header: DialogHeader(
        title: 'Add an Event',
        onClose: () => Navigator.pop(context),
      ),
      body: _buildForm(),
      footer: Row(
        mainAxisAlignment: MainAxisAlignment.end,
        children: [
          FilledButton.icon(
            icon: Icon(Icons.add),
            onPressed: _nameController.text.isEmpty
                ? null
                : () {
                    final successful = _addEvent();
                    if (successful) {
                      Navigator.pop(context);
                    }
                  },
            label: Text("Add"),
          ),
        ],
      ),
    );
  }

  Widget _buildForm() {
    return SingleChildScrollView(
      child: Column(
        children: [
          TextFormField(
            controller: _nameController,
            decoration: const InputDecoration(labelText: 'Name'),
          ),
          TextFormField(
            controller: _locationController,
            decoration: const InputDecoration(labelText: 'Location'),
          ),
          TextFormField(
            controller: _descriptionController,
            decoration: const InputDecoration(labelText: 'Description'),
            maxLines: 3,
          ),
          TextFormField(
            controller: _yearController,
            decoration: const InputDecoration(labelText: 'Year'),
            keyboardType: TextInputType.number,
            inputFormatters: <TextInputFormatter>[
              FilteringTextInputFormatter.digitsOnly
            ],
          ),
          TextFormField(
            controller: _monthController,
            decoration: const InputDecoration(labelText: 'Month'),
            keyboardType: TextInputType.number,
            inputFormatters: <TextInputFormatter>[
              FilteringTextInputFormatter.digitsOnly
            ],
          ),
          TextFormField(
            controller: _dayController,
            decoration: const InputDecoration(labelText: 'Day'),
            keyboardType: TextInputType.number,
            inputFormatters: <TextInputFormatter>[
              FilteringTextInputFormatter.digitsOnly
            ],
          ),
        ],
      ),
    );
  }

  bool _addEvent() {
    final dateNotSelected = _yearController.text.isEmpty &&
        _monthController.text.isEmpty &&
        _dayController.text.isEmpty;
    final year = int.tryParse(_yearController.text) ??
        (dateNotSelected ? widget.campaign.year : 1);
    final month = int.tryParse(_monthController.text) ??
        (dateNotSelected ? widget.campaign.month : 1);
    final day = int.tryParse(_dayController.text) ??
        (dateNotSelected ? widget.campaign.day : 1);

    if (year < 1) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Year must be greater than 0.'),
        ),
      );
      return false;
    }
    if (month < 1 || month > 12) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Month must be between 1 and 12.'),
        ),
      );
      return false;
    }
    if (day < 1 || day > 30) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Day must be between 1 and 30.'),
        ),
      );
      return false;
    }

    _eventService.add(Event(
        id: '',
        campaignId: widget.campaign.id,
        name: _nameController.text,
        description: _descriptionController.text.isEmpty
            ? null
            : _descriptionController.text,
        location:
            _locationController.text.isEmpty ? null : _locationController.text,
        year: year,
        month: month,
        day: day));
    return true;
  }
}
