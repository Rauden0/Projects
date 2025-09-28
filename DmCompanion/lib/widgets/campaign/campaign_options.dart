import 'package:flutter/material.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/widgets/styled_alert.dart';

class CampaignOptions extends StatefulWidget {
  final Campaign campaign;
  final VoidCallback onChangeArchived;
  final VoidCallback onDelete;

  CampaignOptions(
      {super.key,
      required this.campaign,
      required this.onChangeArchived,
      required this.onDelete});

  @override
  State<CampaignOptions> createState() => _CampaignOptionsState();
}

class _CampaignOptionsState extends State<CampaignOptions> {
  final FocusNode _buttonFocusNode = FocusNode(debugLabel: 'Menu Button');

  @override
  void dispose() {
    _buttonFocusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MenuAnchor(
        childFocusNode: _buttonFocusNode,
        menuChildren: [
          MenuItemButton(
            onPressed: widget.onChangeArchived,
            leadingIcon: Icon(Icons.archive),
            child: Text(widget.campaign.archived ? 'Unarchive' : 'Archive'),
          ),
          MenuItemButton(
            onPressed: _showDeleteDialog,
            leadingIcon: Icon(Icons.delete),
            child: Text('Delete'),
          ),
        ],
        child: IconButton(
          onPressed: () {},
          icon: Icon(Icons.expand_more),
        ),
        builder:
            (BuildContext context, MenuController controller, Widget? child) {
          return IconButton(
            focusNode: _buttonFocusNode,
            onPressed: () {
              if (controller.isOpen) {
                controller.close();
              } else {
                controller.open();
              }
            },
            icon: Icon(Icons.more_vert),
          );
        });
  }

  Future<void> _showDeleteDialog() async {
    return showDialog<void>(
      context: context,
      builder: (BuildContext context) {
        return StyledAlert(
            title: 'Delete ${widget.campaign.name}?',
            content: [
              'Are you sure you want to delete this campaign?',
              'This action cannot be undone.'
            ],
            confirmTitle: 'Delete',
            onConfirm: widget.onDelete,
            confirmIcon: Icon(Icons.delete));
      },
    );
  }
}
